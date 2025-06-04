# 🌿 Procedural GPU Grass Rendering in Unity (Built-in RP + Compute Shader)

## Overview

This project implements a **high-performance grass rendering system** in Unity using:

* `ComputeShader` for procedural grass instance generation
* `DrawMeshInstancedIndirect` for efficient GPU instancing
* Fully compatible with **macOS + Metal**, **Unity Built-in RP**

## Demo Features

| Feature                                   | Description                                                                          |
| ----------------------------------------- | ------------------------------------------------------------------------------------ |
| ✅ **Procedural Placement**                | Grass blades are distributed across a plane with spatial jitter for natural density. |
| ✅ **Per-blade Growth Direction**          | Each blade is given a random growth direction vector for dynamic bending.            |
| ✅ **Random Rotation Angle**               | Grass blades are randomly rotated (around Y) for visual variety.                     |
| ✅ **Vertex Wind Animation**               | Wind is simulated in vertex shader using height-based sway with spring damping.      |
| ✅ **Ground Color Modulation**             | Grass color is blended with a terrain texture for local variation.                   |
| ✅ **Dynamic Wind Phase**                  | Wind offset varies by position, creating traveling wave-like motion.                 |
| ✅ **Nonlinear Sway**                      | Higher parts of grass bend more than roots (`pow(height, 1.5)` based sway).          |

---

## 🌿 GPU Instancing for Efficient Grass Rendering

This project utilizes GPU instancing to render vast fields of grass efficiently. By minimizing CPU overhead and draw calls, it achieves high performance even with tens of thousands of grass blades.

### 🚀 What Is GPU Instancing?

GPU instancing is a rendering technique that allows the GPU to draw multiple instances of the same mesh in a single draw call. Each instance can have unique properties, such as position, rotation, scale, or color, enabling variation without additional CPU overhead.

In Unity, GPU instancing can be enabled by checking the "Enable GPU Instancing" option in the material settings. For more advanced control, Unity provides APIs like `Graphics.DrawMeshInstanced`, `Graphics.DrawMeshInstancedProcedural`, and `Graphics.DrawMeshInstancedIndirect`.

### 🌱 Implementation in This Project

In this demo, a single grass mesh is rendered thousands of times across the terrain using GPU instancing. Key aspects include:

* **Instance Data**: Each grass blade's position and other per-instance data are stored in a `ComputeBuffer` and passed to the shader.

* **Draw Call**: The `Graphics.DrawMeshInstancedIndirect` method is used to issue a single draw call for all instances, with the instance count and other parameters provided via a compute buffer.

* **Shader Support**: The shader accesses per-instance data using the `StructuredBuffer` and applies transformations accordingly. It also includes wind animation by offsetting vertices based on noise functions and time.

### 🌬️ Wind Animation

To simulate realistic grass movement, the shader applies a wind effect by offsetting the vertices of each grass blade. This is achieved by:

* **Noise Function**: A noise function generates pseudo-random values based on the world position to ensure that each blade moves differently.

* **Time-Based Animation**: The noise value is combined with a time factor to create continuous, natural-looking sway.

This approach allows for dynamic and varied grass movement without significant performance costs.

### 📈 Performance Benefits

By leveraging GPU instancing, the project significantly reduces the number of draw calls, leading to improved rendering performance. This is especially beneficial when rendering large numbers of identical or similar objects, such as grass, trees, or crowds.

For more information on GPU instancing in Unity, refer to the [Unity Manual on GPU Instancing](https://docs.unity3d.com/Manual/GPUInstancing.html).

---

## 📂 Project Core Structure

```
Assets/
├── Shaders/
│   ├── GrassInstanced.shader         # HLSL shader with wind + gradient + terrain color blending
│   └── GrassCompute.compute          # ComputeShader for grass placement, angle, direction
├── Scripts/
│   └── GrassRenderer.cs              # Main controller for dispatching and rendering
├── Textures/
│   └── GroundTex.png                 # Terrain base color for blending
```

---

## Key Technologies

* `ComputeShader` to generate:

  * `positionBuffer`: xyz + rotation angle
  * `growDirBuffer`: growth direction vector
* `DrawMeshInstancedIndirect`: GPU-side grass rendering
* Custom grass blade mesh generated at runtime (`BuildDiamondGrassBlade`)
* Per-blade sway simulation with spring damping and wind strength

---

### Grass Logic Summary

* Each grass blade is:

  * Planted at jittered grid location
  * Assigned a random rotation (around Y)
  * Tilted slightly in a random upward direction
  * Bent dynamically via a wind function + spring model
  * Colored by height gradient + terrain texture sample

---

## Visual Result

![Grass Screenshot](./Screenshot.png)

---

### 🔮 Possible Extensions

* ✅ Distance-based LOD (already prepared)
* 🌬️ Wind texture distortion (already partially implemented)
* 🌱 Interaction (e.g. characters flatten grass)
* ⛅ Light & shadow support
* 🌾 Varying grass species / color clusters

---

### 💡 Credits & Inspiration

* Inspired by Studio Ghibli-style grass motion 🌱
* Uses math-heavy random hashing for fast procedural placement

---

### 📋 Requirements

* Unity 6+ (Built-in Render Pipeline)
* macOS or Windows
* No URP/HDRP dependencies
