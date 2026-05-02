#ifndef ILLUSION_TYPES_H
#define ILLUSION_TYPES_H

#ifdef _WIN32
#ifdef ILLUSION_NATIVE_EXPORTS
#define ILLUSION_API __declspec(dllexport)
#else
#define ILLUSION_API __declspec(dllimport)
#endif
#else
#define ILLUSION_API extern
#endif

#include <stdint.h>
#include <stdbool.h>

typedef struct {
    const char* id;
    const char* name;
} illusion_object_t;

typedef struct {
    const char* predicate_name;
    illusion_object_t target_object;
} illusion_expression_t;

#endif // ILLUSION_TYPES_H