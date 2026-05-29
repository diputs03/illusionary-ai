#ifndef IPK_KERNEL_H
#define IPK_KERNEL_H

#include "ipk_common.h"
#include "ipk_context.h"

typedef struct IPK_ProofNode IPK_ProofNode;
typedef IPK_ProofNode* ProofNode_Handle;
struct IPK_ProofNode {
    index_t id;
    String_Handle rule_name;
    AST_Handle statement;
    size_t premise_count;
    index_t* premise_ids;
};

typedef struct IPK_ProofDAG IPK_ProofDAG;
typedef IPK_ProofDAG* ProofDAG_Handle;
struct IPK_ProofDAG {
    size_t node_count;
    IPK_ProofNode* nodes;
};

#ifdef __cplusplus
extern "C" {
#endif

    IPK_RESULT IPK_VerifyProof(Context_Handle context,
        ProofDAG_Handle proof_dag,
        bool* out_is_valid);

    void IPK_FreeProofDAG(ProofDAG_Handle proof_dag);

#ifdef __cplusplus
}
#endif

#endif // IPK_KERNEL_H