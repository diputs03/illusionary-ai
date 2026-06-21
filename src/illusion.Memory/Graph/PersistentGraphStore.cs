using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using illusion.CoreLogic.Graph;

namespace illusion.Memory.Graph;

/// <summary>
/// Persistent append-only graph storage with WAL and snapshots
/// Immutable graph + crash recovery + incremental snapshots
/// </summary>
public class PersistentGraphStore : IDisposable
{
    private readonly string _dataDir;
    private readonly string _walPath;
    private readonly string _snapshotDir;
    private readonly ConcurrentQueue<GraphOperation> _writeQueue = new();
    private readonly Task _writeTask;
    private readonly CancellationTokenSource _cts = new();
    private long _lastSnapshotLsn;
    private long _currentLsn;

    // In-memory state
    private KnowledgeGraph.Builder _graphBuilder = KnowledgeGraph.CreateBuilder();
    private KnowledgeGraph _currentGraph;

    public PersistentGraphStore(string dataDir)
    {
        _dataDir = dataDir;
        _walPath = Path.Combine(_dataDir, "graph.wal");
        _snapshotDir = Path.Combine(_dataDir, "snapshots");

        Directory.CreateDirectory(_dataDir);
        Directory.CreateDirectory(_snapshotDir);

        // Recover from disk
        Recover();
        _currentGraph = _graphBuilder.Build();

        // Start background writer
        _writeTask = Task.Run(WriteLoop, _cts.Token);
    }

    #region Public API

    /// <summary>
    /// Append node to graph (immutable, append-only)
    /// </summary>
    public void AppendNode(GraphNode node)
    {
        EnqueueOperation(new GraphOperation
        {
            Lsn = Interlocked.Increment(ref _currentLsn),
            Type = OpType.AddNode,
            Node = node,
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Append edge to graph (immutable, append-only)
    /// </summary>
    public void AppendEdge(GraphEdge edge)
    {
        EnqueueOperation(new GraphOperation
        {
            Lsn = Interlocked.Increment(ref _currentLsn),
            Type = OpType.AddEdge,
            Edge = edge,
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Get current immutable graph snapshot
    /// </summary>
    public KnowledgeGraph GetSnapshot() => _currentGraph;

    /// <summary>
    /// Force checkpoint (create snapshot)
    /// </summary>
    public void Checkpoint()
    {
        // Wait for all writes to complete
        while (_writeQueue.Count > 0)
            Thread.Sleep(10);

        CreateSnapshot();
    }

    #endregion

    #region WAL Implementation

    private void EnqueueOperation(GraphOperation op)
    {
        // Apply to in-memory immediately
        ApplyOperation(op);
        _writeQueue.Enqueue(op);
    }

    private async Task WriteLoop()
    {
        while (!_cts.IsCancellationRequested)
        {
            if (_writeQueue.TryDequeue(out var op))
            {
                // Append to WAL
                await AppendToWAL(op);

                // Auto-snapshot every 1000 operations
                if (op.Lsn - _lastSnapshotLsn > 1000)
                {
                    CreateSnapshot();
                }
            }
            else
            {
                await Task.Delay(10, _cts.Token);
            }
        }
    }

    private async Task AppendToWAL(GraphOperation op)
    {
        var json = JsonSerializer.Serialize(op);
        await File.AppendAllTextAsync(_walPath, json + "\n", _cts.Token);
    }

    private void Recover()
    {
        // 1. Load latest snapshot
        LoadLatestSnapshot();

        // 2. Replay WAL from last snapshot LSN
        if (File.Exists(_walPath))
        {
            foreach (var line in File.ReadLines(_walPath))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                var op = JsonSerializer.Deserialize<GraphOperation>(line);
                if (op != null && op.Lsn > _lastSnapshotLsn)
                {
                    ApplyOperation(op);
                }
            }
        }
    }

    private void ApplyOperation(GraphOperation op)
    {
        switch (op.Type)
        {
            case OpType.AddNode:
                _graphBuilder.AddNode(op.Node!);
                break;
            case OpType.AddEdge:
                _graphBuilder.AddEdge(op.Edge!);
                break;
        }
        // Rebuild graph after each batch? No - only on GetSnapshot
        _currentGraph = _graphBuilder.Build();
    }

    #endregion

    #region Snapshot Implementation

    private void CreateSnapshot()
    {
        var snapshotLsn = _currentLsn;
        var snapshotPath = Path.Combine(_snapshotDir, $"snapshot-{snapshotLsn}.json");

        var snapshot = new GraphSnapshot
        {
            Lsn = snapshotLsn,
            Timestamp = DateTime.UtcNow,
            Nodes = _currentGraph.Nodes.ToList(),
            Edges = _currentGraph.Nodes
                .SelectMany(n => _currentGraph.GetOutEdges(n.Id))
                .Distinct()
                .ToList()
        };

        File.WriteAllText(snapshotPath, JsonSerializer.Serialize(snapshot, new JsonSerializerOptions
        {
            WriteIndented = true
        }));

        _lastSnapshotLsn = snapshotLsn;

        // Clean old WAL (truncate up to snapshot LSN)
        CompactWAL();
    }

    private void LoadLatestSnapshot()
    {
        var snapshots = Directory.GetFiles(_snapshotDir, "snapshot-*.json")
            .Select(f => new { Path = f, Lsn = ExtractLsn(f) })
            .OrderByDescending(s => s.Lsn)
            .FirstOrDefault();

        if (snapshots != null)
        {
            var json = File.ReadAllText(snapshots.Path);
            var snapshot = JsonSerializer.Deserialize<GraphSnapshot>(json);

            if (snapshot != null)
            {
                _graphBuilder = KnowledgeGraph.CreateBuilder();
                _graphBuilder.AddNodes(snapshot.Nodes);
                _graphBuilder.AddEdges(snapshot.Edges);
                _lastSnapshotLsn = snapshot.Lsn;
                _currentLsn = snapshot.Lsn;
            }
        }
    }

    private static long ExtractLsn(string filename)
    {
        var match = System.Text.RegularExpressions.Regex.Match(filename, @"snapshot-(\d+)\.json");
        return match.Success ? long.Parse(match.Groups[1].Value) : 0;
    }

    private void CompactWAL()
    {
        // Keep WAL entries after last snapshot
        if (!File.Exists(_walPath)) return;

        var lines = File.ReadAllLines(_walPath)
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .ToList();

        // Filter without deserializing (avoid FrozenDictionary issue)
        var filtered = new List<string>();
        foreach (var line in lines)
        {
            // Simple LSN extraction from JSON
            var lsnMatch = System.Text.RegularExpressions.Regex.Match(line, @"\""Lsn\"":\s*(\d+)");
            if (lsnMatch.Success)
            {
                var lsn = long.Parse(lsnMatch.Groups[1].Value);
                if (lsn > _lastSnapshotLsn)
                    filtered.Add(line);
            }
        }

        File.WriteAllLines(_walPath, filtered);
    }

    #endregion

    public void Dispose()
    {
        _cts.Cancel();
        try { _writeTask.Wait(1000); } catch { }
        _cts.Dispose();
    }

    #region GGTP / UPP Integration

    /// <summary>
    /// GGTP Pull: Download graph fragments from Global Ground Truth Plane
    /// Merges remote nodes/edges into local graph
    /// </summary>
    public async Task GgtpPull(string ggtpEndpoint, string namespaceFilter = "")
    {
        // TODO: Implement actual GGTP protocol
        // This is the interface placeholder - actual implementation will call GGTP API
        
        // Simulated: Pull high-importance nodes from global graph
        var remoteNodes = await FetchRemoteNodes(ggtpEndpoint, namespaceFilter);
        var remoteEdges = await FetchRemoteEdges(ggtpEndpoint, namespaceFilter);

        foreach (var node in remoteNodes)
        {
            if (!_currentGraph.HasNode(node.Id))
            {
                AppendNode(node);
            }
        }

        foreach (var edge in remoteEdges)
        {
            // Deduplicate edges
            var existing = _currentGraph.GetOutEdges(edge.FromId)
                .Any(e => e.ToId == edge.ToId && e.Type == edge.Type);
            if (!existing)
            {
                AppendEdge(edge);
            }
        }
    }

    /// <summary>
    /// UPP Push: Contribute local graph to User Private Plane
    /// Only pushes nodes/edges created by this user
    /// </summary>
    public async Task UppPush(string uppEndpoint, string userNamespace)
    {
        // TODO: Implement actual UPP protocol
        // This is the interface placeholder - actual implementation will call UPP API
        
        var localNodes = _currentGraph.Nodes
            .Where(n => n.Id.StartsWith(userNamespace))
            .ToList();
        
        var localEdges = _currentGraph.Nodes
            .SelectMany(n => _currentGraph.GetOutEdges(n.Id))
            .Where(e => e.FromId.StartsWith(userNamespace))
            .Distinct()
            .ToList();

        await PushToUpp(uppEndpoint, localNodes, localEdges);
    }

    /// <summary>
    /// Pull by importance level: Only download nodes with importance >= l
    /// Efficient for large graphs - don't download everything
    /// </summary>
    public async Task GgtpPullByImportance(string ggtpEndpoint, double minImportance, string namespaceFilter = "")
    {
        var remoteGraph = await FetchRemoteGraphSummary(ggtpEndpoint, namespaceFilter);
        
        // Only pull nodes that meet importance threshold
        foreach (var node in remoteGraph.Nodes)
        {
            // Calculate remote importance (or fetch from GGTP metadata)
            var estimatedImportance = EstimateRemoteNodeImportance(node);
            if (estimatedImportance >= minImportance)
            {
                if (!_currentGraph.HasNode(node.Id))
                {
                    AppendNode(node);
                }
            }
        }

        // Pull edges between downloaded nodes
        foreach (var edge in remoteGraph.Edges)
        {
            if (_currentGraph.HasNode(edge.FromId) && _currentGraph.HasNode(edge.ToId))
            {
                var existing = _currentGraph.GetOutEdges(edge.FromId)
                    .Any(e => e.ToId == edge.ToId && e.Type == edge.Type);
                if (!existing)
                {
                    AppendEdge(edge);
                }
            }
        }
    }

    #region GGTP/UPP Helpers (Internal)

    private Task<List<GraphNode>> FetchRemoteNodes(string endpoint, string ns)
    {
        // TODO: Actual HTTP call to GGTP
        return Task.FromResult(new List<GraphNode>());
    }

    private Task<List<GraphEdge>> FetchRemoteEdges(string endpoint, string ns)
    {
        // TODO: Actual HTTP call to GGTP
        return Task.FromResult(new List<GraphEdge>());
    }

    private Task<(List<GraphNode> Nodes, List<GraphEdge> Edges)> FetchRemoteGraphSummary(string endpoint, string ns)
    {
        // TODO: Actual HTTP call to GGTP
        return Task.FromResult((new List<GraphNode>(), new List<GraphEdge>()));
    }

    private Task PushToUpp(string endpoint, List<GraphNode> nodes, List<GraphEdge> edges)
    {
        // TODO: Actual HTTP call to UPP
        return Task.CompletedTask;
    }

    private double EstimateRemoteNodeImportance(GraphNode remoteNode)
    {
        // GGTP should provide importance metadata
        // For now, estimate based on node type
        return remoteNode.Type switch
        {
            "Axiom" => 3.0,
            "Rule" => 2.5,
            "Predicate" => 2.0,
            "Object" => 1.0,
            _ => 0.5
        };
    }

    #endregion
    #endregion
}

#region Persistence Types

public enum OpType
{
    AddNode,
    AddEdge
}

public class GraphOperation
{
    public long Lsn { get; set; }
    public OpType Type { get; set; }
    public GraphNode? Node { get; set; }
    public GraphEdge? Edge { get; set; }
    public DateTime Timestamp { get; set; }
}

public class GraphSnapshot
{
    public long Lsn { get; set; }
    public DateTime Timestamp { get; set; }
    public List<GraphNode> Nodes { get; set; } = new();
    public List<GraphEdge> Edges { get; set; } = new();
}

#endregion
