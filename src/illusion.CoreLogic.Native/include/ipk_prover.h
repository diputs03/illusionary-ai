#ifndef IPK_PROVER_H
#define IPK_PROVER_H

#include "ipk_common.h"
#include "ipk_context.h"

typedef struct IPK_ProofNode IPK_ProofNode;
typedef IPK_ProofNode* IPK_ProofNode_Handle;
struct IPK_ProofNode {
    index_t id;
    IPK_String rule_name;
    IPK_AST_Handle statement;
    size_t premise_count;
    index_t* premise_ids;
};

typedef struct IPK_ProofDAG IPK_ProofDAG;
typedef IPK_ProofDAG* IPK_ProofDAG_Handle;
struct IPK_ProofDAG {
    size_t node_count;
    IPK_ProofNode* nodes;
};

#ifdef __cplusplus
extern "C" {
#endif

    IAPI_EXPORT IPK_RESULT IPK_VerifyProof(IPK_Context_Handle context, IPK_ProofDAG_Handle proof_dag, bool* out_is_valid);
    IAPI_EXPORT void IPK_FreeProofDAG(IPK_ProofDAG_Handle proof_dag);

#ifdef __cplusplus
}
#endif

#endif // IPK_PROVER_H