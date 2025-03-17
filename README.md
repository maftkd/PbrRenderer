# PBR Renderer

![pbrDemo](https://github.com/user-attachments/assets/24a3f572-98ef-4f46-af4b-eb6f89d00ec2)

This repo is based heavily off the tutorials on [learnopengl](https://learnopengl.com/PBR/Theory). In fact this project was really meant as a learning experience for myself into the basics of PBR. 

This project implements a metallic-flow using Cook-Torrence BRDF as described on learnopengl.

I implemented my own indirect diffuse and specular environment map baker. This project supports baking environment maps at runtime and allows users to quickly switch between various environments.

I also added the ability to click on objects and point light sources in order to move and manipulate them. The translation gizmo work here would later be used in other projects like [PathTracer](https://github.com/maftkd/PathTracer) & [MeshEditor](https://github.com/maftkd/MeshEditor) in order to provide ways of translating elements at runtime.
