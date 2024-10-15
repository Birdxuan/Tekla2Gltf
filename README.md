##  Tekla2Gltf 

[English](doc/README_EN.md) | [中文](doc/README_CN.md)

### I. Tekla2glTF: A Tekla glTF Exporter

This project aims to explore and understand the glTF/glb 3D data format, focusing primarily on model data format processing. As the author is not a professional BIM software user, the emphasis is solely on exporting model data formats.

### II. Current Features

* Normal vector export
* Basic material export
* Toggle switch for selecting export method

### III. TODO

* Property export

### IV. Contact

For any inquiries, please contact: Birdxuan@163.com

### About

An open-source glTF format exporter for Trimble's Tekla software
This project is developed using Tekla 2022, but it doesn't heavily rely on the Tekla API. It supports other versions as well.

### Usage:

1. Open Tekla 3D view

2. Select format and path for export

2. <iframe       width="100%"       height="450"       src="doc/LookLook.mp4"       scrolling="no"       border="0"       frameborder="no"       framespacing="0"       allowfullscreen="true">  。  </iframe> 

   4.Download

```
https://warehouse.tekla.com/#/collections/online/ub9b111ad-68fc-46a1-a6da-1cf3010ebd84
```



### Compilation

This project uses two open-source libraries for mesh triangulation. Pre-compiled APIs are available, but you can also add these projects for further learning:
a. https://github.com/wo80/Triangle.NET.git
b. https://github.com/gradientspace/geometry3Sharp

Note: The Tekla API libraries are already compiled, but you have the option to include these projects for deeper understanding and exploration.