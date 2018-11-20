// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

#include "gtest/gtest.h"
#include "test/providers/provider_test_utils.h"

namespace onnxruntime {
namespace test {

TEST(CompressTest, Compress0) {
  OpTester test("Compress", 9);

  test.AddAttribute("axis", int64_t(0));

  test.AddInput<float>("input", {3, 2}, {
      1.0f, 2.0f,
      3.0f, 4.0f,
      5.0f, 6.0f});
  test.AddInput<bool>("condition", {3}, {0, 1, 1});
  test.AddOutput<float>("output", {2, 2}, {
      3.0f, 4.0f,
      5.0f, 6.0f});
  test.Run();
}

TEST(CompressTest, Compress1) {
  OpTester test("Compress", 9);

  test.AddAttribute("axis", int64_t(1));

  test.AddInput<float>("input", {3, 2}, {
      1.0f, 2.0f,
      3.0f, 4.0f,
      5.0f, 6.0f});
  test.AddInput<bool>("condition", {2}, {0, 1});
  test.AddOutput<float>("output", {3, 1}, {
      2.0f,
      4.0f,
      6.0f});
  test.Run();
}

TEST(CompressTest, Compress_3dims) {
  OpTester test("Compress", 9);

  test.AddAttribute("axis", int64_t(1));

  test.AddInput<float>("input", {2, 2, 3}, {
      1.0f, 2.0f, 3.0f,
      4.0f, 5.0f, 6.0f,

      7.0f, 8.0f, 9.0f,
      10.0f, 11.0f, 12.0f});
  test.AddInput<bool>("condition", {2}, {0, 1});
  test.AddOutput<float>("output", {2, 1, 3}, {
      4.0f, 5.0f, 6.0f,
      10.0f, 11.0f, 12.0f});
  test.Run();
}

TEST(CompressTest, Compress_condition_all_false) {
  OpTester test("Compress", 9);

  test.AddAttribute("axis", int64_t(1));

  test.AddInput<float>("input", {2, 2, 3}, {1.0f, 2.0f, 3.0f,
                                            4.0f, 5.0f, 6.0f,

                                            7.0f, 8.0f, 9.0f,
                                            10.0f, 11.0f, 12.0f});
  test.AddInput<bool>("condition", {2}, {0, 0});
  test.AddOutput<float>("output", {2, 0, 3}, {});
  test.Run();
}

TEST(CompressTest, Compress_3dims_has_extra_condition) {
  OpTester test("Compress", 9);

  test.AddAttribute("axis", int64_t(1));

  test.AddInput<float>("input", {2, 2, 3}, {
      1.0f, 2.0f, 3.0f,
      4.0f, 5.0f, 6.0f,

      7.0f, 8.0f, 9.0f,
      10.0f, 11.0f, 12.0f});
  // has condition length = 3 > input_dim[axis] = 2
  test.AddInput<bool>("condition", {3}, {0, 1, 1});
  test.AddOutput<float>("output", {2, 1, 3}, {
      4.0f, 5.0f, 6.0f,
      10.0f, 11.0f, 12.0f});
  test.Run();
}

TEST(CompressTest, Compress_3dims_has_extra_input) {
  OpTester test("Compress", 9);

  test.AddAttribute("axis", int64_t(1));

  test.AddInput<float>("input", {2, 3, 3}, {
      1.0f, 2.0f, 3.0f,
      4.0f, 5.0f, 6.0f,
      7.0f, 8.0f, 9.0f,

      10.0f, 11.0f, 12.0f,
      13.0f, 14.0f, 15.0f,
      16.0f, 17.0f, 18.0f});
  // has condition length = 2 < input_dim[axis] = 3
  test.AddInput<bool>("condition", {2}, {0, 1});
  test.AddOutput<float>("output", {2, 1, 3}, {
      4.0f, 5.0f, 6.0f,
      13.0f, 14.0f, 15.0f});
  test.Run();
}

TEST(CompressTest, Compress_default_axis) {
  OpTester test("Compress", 9);
  
  test.AddInput<float>("input", {3, 2}, {
      1.0f, 2.0f,
      3.0f, 4.0f,
      5.0f, 6.0f});
  test.AddInput<bool>("condition", {5}, {0, 1, 0, 0, 1});
  test.AddOutput<float>("output", {2}, {2.0f, 5.0f});
  test.Run();
}

TEST(CompressTest, Compress0_string) {
  OpTester test("Compress", 9);

  test.AddAttribute("axis", int64_t(0));

  test.AddInput<std::string>("input", {3, 2}, {
      "1", "2",
      "3", "4",
      "5", "6"});
  test.AddInput<bool>("condition", {3}, {0, 1, 1});
  test.AddOutput<std::string>("output", {2, 2}, {
      "3", "4",
      "5", "6"});
  test.Run();
}

TEST(CompressTest, Compress_default_axis_string) {
  OpTester test("Compress", 9);
  
  test.AddInput<std::string>("input", {3, 2}, {
      "1", "2",
      "3", "4",
      "5", "6"});
  test.AddInput<bool>("condition", {5}, {0, 1, 0, 0, 1});
  test.AddOutput<std::string>("output", {2}, {"2", "5"});
  test.Run();
}

}  // namespace Test
}  // namespace onnxruntime
