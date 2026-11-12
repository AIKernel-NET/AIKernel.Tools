/* Test-only ABI fixtures. These are never distributed as AIKernel runtime assets. */
#include <stdint.h>
#include <math.h>
#ifdef _WIN32
#define EXPORT __declspec(dllexport)
#else
#define EXPORT __attribute__((visibility("default")))
#endif
EXPORT uint32_t ak_abi_version(void) {
#ifdef FIXTURE_wrong_version
    return 2;
#else
    return 1;
#endif
}
EXPORT int32_t ak_softmax(void *context, float *values, uint32_t count) {
    (void)context;
    if (!values || !count) {
#ifdef FIXTURE_accepts_invalid
        return 0;
#else
        return 1;
#endif
    }
    if (count != 2) return 1;
#ifdef FIXTURE_wrong_result
    values[0] = values[1] = 0;
#elif defined(FIXTURE_nonfinite_result)
    values[0] = values[1] = NAN;
#else
    values[0] = 0.880797078f; values[1] = 0.119202922f;
#endif
    return 0;
}
/* Verifier checks these exports exist but must not call them (especially GPU discovery). */
#include <stdlib.h>
#define UNUSED(name) EXPORT void name(void) { abort(); }
UNUSED(ak_get_device_count) UNUSED(ak_create) UNUSED(ak_destroy) UNUSED(ak_get_stats)
UNUSED(ak_device_alloc) UNUSED(ak_device_free) UNUSED(ak_pinned_alloc) UNUSED(ak_pinned_free)
UNUSED(ak_upload) UNUSED(ak_upload_shared) UNUSED(ak_wipe_device) UNUSED(ak_wipe_scratch)
UNUSED(ak_get_pci_bus_id) UNUSED(ak_download) UNUSED(ak_gemv) UNUSED(ak_normalize)
UNUSED(ak_rope) UNUSED(ak_swiglu)
#ifndef FIXTURE_missing_export
UNUSED(ak_attention_tile)
#endif
