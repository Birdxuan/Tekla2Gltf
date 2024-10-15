#  Tekla2Gltf 

[English](README_EN.md) | [中文](README_CN.md)

### I. Tekla2glTF：Tekla glTF 导出器
本项目旨在探索和理解 glTF/glb 3D 数据格式，主要关注模型数据格式处理。由于作者不是专业的 BIM 软件用户，因此重点仅放在导出模型数据格式上。

### II. 当前功能
* 法向量导出
* 基本材质导出
* 选择导出方式的切换开关

### III. 待办事项
* 属性导出

### IV. 联系方式
如有任何疑问，请联系：Birdxuan@163.com

### 关于
这是一个用于 Trimble 公司 Tekla 软件的开源 glTF 格式导出器
本项目使用 Tekla 2022 开发，但并不严重依赖 Tekla API。它也支持其他版本。

### 使用方法：
1. 打开 Tekla 3D 视图

2. 选择导出格式和路径

2. <iframe       width="100%"       height="450"       src="LookLook.mp4"       scrolling="no"       border="0"       frameborder="no"       framespacing="0"       allowfullscreen="true">  。  </iframe> 

   4.模型下载

```
https://warehouse.tekla.com/#/collections/online/ub9b111ad-68fc-46a1-a6da-1cf3010ebd84
```



### 编译

本项目使用两个开源库进行网格三角化。提供了预编译的 API，但您也可以添加这些项目以进行进一步学习：
a. https://github.com/wo80/Triangle.NET.git
b. https://github.com/gradientspace/geometry3Sharp

注意：Tekla API 库已经编译好，但您可以选择包含这些项目以便更深入地理解和探索。