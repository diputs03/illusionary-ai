#ifndef ILLUSION_CRYPTO_H
#define ILLUSION_CRYPTO_H

#include "illusion_types.h"

#ifdef __cplusplus
extern "C" {
#endif

    ILLUSION_API bool illusion_crypto_sha256(
        const uint8_t* input_data,
        size_t input_length,
        uint8_t* output_hash,
        size_t output_hash_size
    );

    ILLUSION_API bool illusion_crypto_verify_signature(
        const uint8_t* module_data,
        size_t module_length,
        const uint8_t* signature,
        size_t signature_length,
        const uint8_t* public_key,
        size_t public_key_length
    );

#ifdef __cplusplus
}
#endif

#endif // ILLUSION_CRYPTO_H