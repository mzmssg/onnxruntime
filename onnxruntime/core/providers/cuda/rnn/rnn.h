// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

#pragma once

#include "cudnn_rnn_base.h"
#include "gsl/gsl_util"
#include "core/providers/cuda/cuda_common.h"
#include <cudnn.h>

namespace onnxruntime {
namespace cuda {

template <typename T>
class RNN final : public CudnnRnnBase<T> {
  const std::set<std::string> allowed_activations{"Relu", "Tanh" /*, "Sigmoid"*/};

 public:
  RNN(const OpKernelInfo& info) : CudnnRnnBase<T>(info) {
    std::vector<std::string> activations_;
    ONNXRUNTIME_ENFORCE(info.GetAttrs("activations", activations_).IsOK());
    if (activations_[0] == "Relu")
      CudnnRnnBase<T>::rnn_mode_ = CUDNN_RNN_RELU;
    else if (activations_[0] == "Tanh")
      CudnnRnnBase<T>::rnn_mode_ = CUDNN_RNN_TANH;

    CudnnRnnBase<T>::SetCudnnRnnDesc();

    // ONNX W mapping to RNNLinLayerMatrixParams the linLayerID is 0
    CudnnRnnBase<T>::W_lin_layer_id_.assign({0});
    // ONNX R mapping to RNNLinLayerMatrixParams the linLayerID is 1
    CudnnRnnBase<T>::R_lin_layer_id_.assign({1});
    // ONNX B layout is Wb, Rb, mapping to RNNLinLayerMatrixParams
    // the linLayerID is 0, 1, we can reuse it from W_lin_layer_id & R_lin_layer_id

    CudnnRnnBase<T>::CacheCudnnRnnWeights(info);
  }
};

}  // namespace cuda
}  // namespace onnxruntime
