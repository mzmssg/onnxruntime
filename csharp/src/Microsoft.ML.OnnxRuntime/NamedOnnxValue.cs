// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics.Tensors;
using System.Buffers;
using System.Collections;
using System.Diagnostics;

namespace Microsoft.ML.OnnxRuntime
{
    public class NamedOnnxValue
    {
        protected Object _value;
        protected string _name;

        public NamedOnnxValue(string name, Object value)
        {
            _name = name;
            _value = value;
        }

        public string Name { get { return _name; } }
        public Tensor<T> AsTensor<T>()
        {
            return _value as Tensor<T>;  // will return null if not castable
        }

        /// <summary>
        /// Attempts to Pin the buffer, and create a native OnnxValue out of it. the pinned MemoryHandle is passed to output.
        /// In this case, the pinnedHandle should be kept alive till the native OnnxValue is used, then dispose it.
        /// If it is not possible to Pin the buffer, then creates OnnxValue from the copy of the data. The output pinnedMemoryHandle
        /// contains a default value in that case.
        /// Attempts to infer the type of the value while creating the OnnxValue
        /// </summary>
        /// <param name="onnxValue"></param>
        /// <param name="pinnedMemoryHandle"></param>
        internal void ToNativeOnnxValue(out IntPtr onnxValue, out MemoryHandle pinnedMemoryHandle)
        {
            //try to cast _value to Tensor<T>
            TensorElementType nativeElementType = TensorElementType.DataTypeMax; //invalid
            IntPtr dataBufferPointer = IntPtr.Zero;
            int dataBufferLength = 0;
            ReadOnlySpan<int> shape = null;
            int rank = 0;
            onnxValue = IntPtr.Zero;

            if (TryPinAsTensor<float>(out pinnedMemoryHandle,
                                      out dataBufferPointer,
                                      out dataBufferLength,
                                      out shape,
                                      out rank,
                                      out nativeElementType
                                    ))
            {
            }
            else if (TryPinAsTensor<double>(out pinnedMemoryHandle,
                                      out dataBufferPointer,
                                      out dataBufferLength,
                                      out shape,
                                      out rank,
                                      out nativeElementType
                                    ))
            {
            }
            else if (TryPinAsTensor<int>(out pinnedMemoryHandle,
                                      out dataBufferPointer,
                                      out dataBufferLength,
                                      out shape,
                                      out rank,
                                      out nativeElementType
                                    ))
            {
            }
            else if (TryPinAsTensor<uint>(out pinnedMemoryHandle,
                                      out dataBufferPointer,
                                      out dataBufferLength,
                                      out shape,
                                      out rank,
                                      out nativeElementType
                                    ))
            {
            }
            else if (TryPinAsTensor<long>(out pinnedMemoryHandle,
                                      out dataBufferPointer,
                                      out dataBufferLength,
                                      out shape,
                                      out rank,
                                      out nativeElementType
                                    ))
            {
            }
            else if (TryPinAsTensor<ulong>(out pinnedMemoryHandle,
                                      out dataBufferPointer,
                                      out dataBufferLength,
                                      out shape,
                                      out rank,
                                      out nativeElementType
                                    ))
            {
            }
            else if (TryPinAsTensor<short>(out pinnedMemoryHandle,
                                      out dataBufferPointer,
                                      out dataBufferLength,
                                      out shape,
                                      out rank,
                                      out nativeElementType
                                    ))
            {
            }
            else if (TryPinAsTensor<ushort>(out pinnedMemoryHandle,
                                      out dataBufferPointer,
                                      out dataBufferLength,
                                      out shape,
                                      out rank,
                                      out nativeElementType
                                    ))
            {
            }
            else if (TryPinAsTensor<byte>(out pinnedMemoryHandle,
                                      out dataBufferPointer,
                                      out dataBufferLength,
                                      out shape,
                                      out rank,
                                      out nativeElementType
                                    ))
            {
            }
            else if (TryPinAsTensor<bool>(out pinnedMemoryHandle,
                                      out dataBufferPointer,
                                      out dataBufferLength,
                                      out shape,
                                      out rank,
                                      out nativeElementType
                                    ))
            {
            }

            //TODO: add other types
            else
            {
                // nothing to cleanup here, since no memory has been pinned
                throw new NotSupportedException("The inference value " + nameof(_value) + " is not of a supported type");
            }


            Debug.Assert(dataBufferPointer != IntPtr.Zero, "dataBufferPointer must be non-null after obtaining the pinned buffer");

            // copy to an ulong[] shape to match size_t[]
            ulong[] longShape = new ulong[rank];
            for (int i = 0; i < rank; i++)
            {
                longShape[i] = (ulong)shape[i];
            }

            IntPtr status = NativeMethods.ONNXRuntimeCreateTensorWithDataAsONNXValue(
                    NativeMemoryAllocatorInfo.DefaultInstance.Handle,
                    dataBufferPointer,
                    (ulong)(dataBufferLength),
                    longShape,
                    (ulong)rank,
                    nativeElementType,
                    out onnxValue
                );
            try
            {
                NativeApiStatus.VerifySuccess(status);
            }
            catch (OnnxRuntimeException e)
            {
                pinnedMemoryHandle.Dispose();
                throw e;
            }

        }

        internal static NamedOnnxValue CreateFromOnnxValue(string name, IntPtr nativeOnnxValue)
        {
            NamedOnnxValue result = null;

            if (true /* TODO: check native data type when API available. assuming Tensor<float> for now */)
            {
                NativeOnnxTensorMemory<float> nativeTensorWrapper = new NativeOnnxTensorMemory<float>(nativeOnnxValue);
                DenseTensor<float> dt = new DenseTensor<float>(nativeTensorWrapper.Memory, nativeTensorWrapper.Dimensions);
                result = new NamedOnnxValue(name, dt);
            }

            return result;
        }

        private bool TryPinAsTensor<T>(
                out MemoryHandle pinnedMemoryHandle,
                out IntPtr dataBufferPointer,
                out int dataBufferLength,
                out ReadOnlySpan<int> shape,
                out int rank,
                out TensorElementType nativeElementType
            )
        {
            nativeElementType = TensorElementType.DataTypeMax; //invalid
            dataBufferPointer = IntPtr.Zero;
            dataBufferLength = 0;
            shape = null;
            rank = 0;
            pinnedMemoryHandle = default(MemoryHandle);

            if (_value is Tensor<T>)
            {
                Tensor<T> t = _value as Tensor<T>;
                if (t.IsReversedStride)
                {
                    //TODO: not sure how to support reverse stride. may be able to calculate the shape differently
                    throw new NotSupportedException(nameof(Tensor<T>) + " of reverseStride is not supported");
                }

                DenseTensor<T> dt = null;
                if (_value is DenseTensor<T>)
                {
                    dt = _value as DenseTensor<T>;
                }
                else
                {
                    dt = t.ToDenseTensor();
                }

                shape = dt.Dimensions;  // does not work for reverse stride
                rank = dt.Rank;
                pinnedMemoryHandle = dt.Buffer.Pin();
                unsafe
                {
                    dataBufferPointer = (IntPtr)pinnedMemoryHandle.Pointer;
                }

                // find the native type
                if (typeof(T) == typeof(float))
                {
                    nativeElementType = TensorElementType.Float;
                    dataBufferLength = dt.Buffer.Length * sizeof(float);
                }
                else if (typeof(T) == typeof(double))
                {
                    nativeElementType = TensorElementType.Double;
                    dataBufferLength = dt.Buffer.Length * sizeof(double);
                }
                else if (typeof(T) == typeof(int))
                {
                    nativeElementType = TensorElementType.Int32;
                    dataBufferLength = dt.Buffer.Length * sizeof(int);
                }
                else if (typeof(T) == typeof(uint))
                {
                    nativeElementType = TensorElementType.UInt32;
                    dataBufferLength = dt.Buffer.Length * sizeof(uint);
                }
                else if (typeof(T) == typeof(long))
                {
                    nativeElementType = TensorElementType.Int64;
                    dataBufferLength = dt.Buffer.Length * sizeof(long);
                }
                else if (typeof(T) == typeof(ulong))
                {
                    nativeElementType = TensorElementType.UInt64;
                    dataBufferLength = dt.Buffer.Length * sizeof(ulong);
                }
                else if (typeof(T) == typeof(short))
                {
                    nativeElementType = TensorElementType.Int16;
                    dataBufferLength = dt.Buffer.Length * sizeof(short);
                }
                else if (typeof(T) == typeof(ushort))
                {
                    nativeElementType = TensorElementType.UInt16;
                    dataBufferLength = dt.Buffer.Length * sizeof(ushort);
                }
                else if (typeof(T) == typeof(byte))
                {
                    nativeElementType = TensorElementType.UInt8;
                    dataBufferLength = dt.Buffer.Length * sizeof(byte);
                }
                //TODO: Not supporting boolean for now. bool is non-blittable, the interop needs some care, and possibly need to copy
                //else if (typeof(T) == typeof(bool))
                //{
                //}
                else
                {
                    //TODO: may extend the supported types
                    // do not throw exception, rather assign the sentinel value
                    nativeElementType = TensorElementType.DataTypeMax;
                }
                return true;
            }

            return false;
        }

        // may expose different types of getters in future

    }

    internal enum TensorElementType
    {
        Float = 1,
        UInt8 = 2,
        Int8 = 3,
        UInt16 = 4,
        Int16 = 5,
        Int32 = 6,
        Int64 = 7,
        String = 8,
        Bool = 9,
        Float16 = 10,
        Double = 11,
        UInt32 = 12,
        UInt64 = 13,
        Complex64 = 14,
        Complex128 = 15,
        BFloat16 = 16,
        DataTypeMax = 17
    }

}
