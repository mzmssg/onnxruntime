// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

#pragma once

#include <gsl/gsl>

#include "core/optimizer/graph_transformer.h"

namespace onnxruntime {
struct FreeDimensionOverride;

namespace transformer_utils {

/** Generates all pre-training transformers for this level. */
std::vector<std::unique_ptr<GraphTransformer>> GeneratePreTrainingTransformers(TransformerLevel level,
                                                                               const std::unordered_set<std::string>& weights_to_train,
                                                                               const std::vector<std::string>& rules_and_transformers_to_enable = {});

/** Generates all predefined (both rule-based and non-rule-based) transformers for this level.
    If transformers_and_rules_to_enable is not empty, it returns the intersection between the predefined transformers/rules 
    and the transformers_and_rules_to_enable. */
std::vector<std::unique_ptr<GraphTransformer>> GenerateTransformers(TransformerLevel level,
                                                                    gsl::span<const FreeDimensionOverride> free_dimension_overrides,
                                                                    const std::vector<std::string>& rules_and_transformers_to_enable = {});

}  // namespace transformer_utils
}  // namespace onnxruntime
