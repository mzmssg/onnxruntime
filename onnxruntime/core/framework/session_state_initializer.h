// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

#pragma once
#include <map>

#include "core/framework/allocator.h"
#include "core/framework/tensor.h"

namespace onnxruntime {
class Graph;
class GraphTransformerManager;
}  // namespace onnxruntime

namespace onnxruntime {
class SessionState;
class ExecutionProviders;
class KernelRegistryManager;
class InsertCastTransformer;

namespace logging {
class Logger;
}

class SessionStateInitializer {
 public:
  SessionStateInitializer(onnxruntime::Graph& graph,
                          SessionState& session_state,
                          const ExecutionProviders& providers,
                          KernelRegistryManager& kernel_registry_manager,
                          const logging::Logger& logger);

  // First perform any transformations and create the execution plan
  common::Status CreatePlan(const onnxruntime::GraphTransformerManager& graph_transformation_manager,
                            const InsertCastTransformer& insert_cast_transformer,
                            bool enable_sequential_execution);

  // initialize tensors, and save. save kernels and input/output node mappings
  // @param enable_memory_pattern
  common::Status InitializeAndSave(bool enable_memory_pattern,
                                   std::map<ONNXRuntimeAllocatorInfo, BufferUniquePtr>& weights_buffers);

 private:
  onnxruntime::Graph& graph_;
  SessionState& session_state_;

  const ExecutionProviders& execution_providers_;
  KernelRegistryManager& kernel_registry_manager_;
  const logging::Logger& logger_;
};
}  // namespace onnxruntime
