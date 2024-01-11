﻿using System.Diagnostics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace LampyrisStockTradeSystem;

public struct Vertex
{
    public const int Size = (4 + 4) * 4; // size of struct in bytes

    private readonly Vector4 _position;
    private readonly Color4 _color;

    public Vertex(Vector4 position, Color4 color)
    {
        _position = position;
        _color = color;
    }
}

public sealed class RotatingCubeDrawer
{
    private int _width;
    private int _height;

    private int _program;
    private bool _initialized;
    private int _vertexArray;
    private int _buffer;
    private int _verticeCount;

    private Matrix4 _model;
    private Matrix4 _view;
    private Matrix4 _projection;
    private readonly float _FOV = 45.0f;

    private float _lastTimestamp = Stopwatch.GetTimestamp();
    private readonly float _freq = Stopwatch.Frequency;

    private float _angle;

    public void OnLoad()
    {
        _model = Matrix4.Identity;
        Vertex[] vertices =
        {
            new Vertex(new Vector4(-0.5f, -0.5f, -0.5f,  1.0f), Color4.Blue),
            new Vertex(new Vector4( 0.5f, -0.5f, -0.5f,  1.0f), Color4.Blue),
            new Vertex(new Vector4( 0.5f,  0.5f, -0.5f,  1.0f), Color4.Blue),
            new Vertex(new Vector4( 0.5f,  0.5f, -0.5f,  1.0f), Color4.Blue),
            new Vertex(new Vector4(-0.5f,  0.5f, -0.5f,  1.0f), Color4.Blue),
            new Vertex(new Vector4(-0.5f, -0.5f, -0.5f,  1.0f), Color4.Blue),

            new Vertex(new Vector4(-0.5f, -0.5f,  0.5f,  1.0f), Color4.Blue),
            new Vertex(new Vector4( 0.5f, -0.5f,  0.5f,  1.0f), Color4.Blue),
            new Vertex(new Vector4( 0.5f,  0.5f,  0.5f,  1.0f), Color4.Blue),
            new Vertex(new Vector4( 0.5f,  0.5f,  0.5f,  1.0f), Color4.Blue),
            new Vertex(new Vector4(-0.5f,  0.5f,  0.5f,  1.0f), Color4.Blue),
            new Vertex(new Vector4(-0.5f, -0.5f,  0.5f,  1.0f), Color4.Blue),

            new Vertex(new Vector4(-0.5f,  0.5f,  0.5f,  1.0f), Color4.Red),
            new Vertex(new Vector4(-0.5f,  0.5f, -0.5f,  1.0f), Color4.Red),
            new Vertex(new Vector4(-0.5f, -0.5f, -0.5f,  1.0f), Color4.Red),
            new Vertex(new Vector4(-0.5f, -0.5f, -0.5f,  1.0f), Color4.Red),
            new Vertex(new Vector4(-0.5f, -0.5f,  0.5f,  1.0f), Color4.Red),
            new Vertex(new Vector4(-0.5f,  0.5f,  0.5f,  1.0f), Color4.Red),

            new Vertex(new Vector4( 0.5f,  0.5f,  0.5f,  1.0f), Color4.Red),
            new Vertex(new Vector4( 0.5f,  0.5f, -0.5f,  1.0f), Color4.Red),
            new Vertex(new Vector4( 0.5f, -0.5f, -0.5f,  1.0f), Color4.Red),
            new Vertex(new Vector4( 0.5f, -0.5f, -0.5f,  1.0f), Color4.Red),
            new Vertex(new Vector4( 0.5f, -0.5f,  0.5f,  1.0f), Color4.Red),
            new Vertex(new Vector4( 0.5f,  0.5f,  0.5f,  1.0f), Color4.Red),

            new Vertex(new Vector4(-0.5f, -0.5f, -0.5f,  1.0f), Color4.Green),
            new Vertex(new Vector4( 0.5f, -0.5f, -0.5f,  1.0f), Color4.Green),
            new Vertex(new Vector4( 0.5f, -0.5f,  0.5f,  1.0f), Color4.Green),
            new Vertex(new Vector4( 0.5f, -0.5f,  0.5f,  1.0f), Color4.Green),
            new Vertex(new Vector4(-0.5f, -0.5f,  0.5f,  1.0f), Color4.Green),
            new Vertex(new Vector4(-0.5f, -0.5f, -0.5f,  1.0f), Color4.Green),

            new Vertex(new Vector4(-0.5f,  0.5f, -0.5f,  1.0f), Color4.Green),
            new Vertex(new Vector4( 0.5f,  0.5f, -0.5f,  1.0f), Color4.Green),
            new Vertex(new Vector4( 0.5f,  0.5f,  0.5f,  1.0f), Color4.Green),
            new Vertex(new Vector4( 0.5f,  0.5f,  0.5f,  1.0f), Color4.Green),
            new Vertex(new Vector4(-0.5f,  0.5f,  0.5f,  1.0f), Color4.Green),
            new Vertex(new Vector4(-0.5f,  0.5f, -0.5f,  1.0f), Color4.Green),
        };

        _verticeCount = vertices.Length;
        _vertexArray = GL.GenVertexArray();
        _buffer = GL.GenBuffer();

        GL.BindVertexArray(_vertexArray);
        GL.BindBuffer(BufferTarget.ArrayBuffer, _buffer);

        // create first buffer: vertex
        GL.NamedBufferStorage(
            _buffer,
            Vertex.Size * vertices.Length,        // the size needed by this buffer
            vertices,                           // data to initialize with
            BufferStorageFlags.MapWriteBit);    // at this point we will only write to the buffer


        GL.VertexArrayAttribBinding(_vertexArray, 0, 0);
        GL.EnableVertexArrayAttrib(_vertexArray, 0);
        GL.VertexArrayAttribFormat(
            _vertexArray,
            0,                      // attribute index, from the shader location = 0
            4,                      // size of attribute, vec4
            VertexAttribType.Float, // contains floats
            false,                  // does not need to be normalized as it is already, floats ignore this flag anyway
            0);                     // relative offset, first item


        GL.VertexArrayAttribBinding(_vertexArray, 1, 0);
        GL.EnableVertexArrayAttrib(_vertexArray, 1);
        GL.VertexArrayAttribFormat(
            _vertexArray,
            1,                      // attribute index, from the shader location = 1
            4,                      // size of attribute, vec4
            VertexAttribType.Float, // contains floats
            false,                  // does not need to be normalized as it is already, floats ignore this flag anyway
            16);                     // relative offset after a vec4

        // link the vertex array and buffer and provide the stride as size of Vertex
        GL.VertexArrayVertexBuffer(_vertexArray, 0, _buffer, IntPtr.Zero, Vertex.Size);
        _initialized = true;

        try
        {
            _program = GL.CreateProgram();
            var shaders = new List<int>();
            ShaderType type = ShaderType.VertexShader;
            var shader = GL.CreateShader(type);
            string src = @"#version 330 core
                                layout (location = 0) in vec4 position;
                                layout(location = 1) in vec4 color;
                                out vec4 vs_color;

                                out vec3 original_normal;
                                out vec3 transformed_normal;

                                uniform mat4 model;
                                uniform mat4 view;
                                uniform mat4 projection;

                                void main(void)
                                {
                                gl_Position = projection * view * model * position;
                                vs_color = color;
                                original_normal = vec3(color);
                                mat3 normal_matrix = transpose(inverse(mat3(view * model)));
                                transformed_normal = normal_matrix * original_normal;
                            }";
            GL.ShaderSource(shader, src);
            GL.CompileShader(shader);
            var info = GL.GetShaderInfoLog(shader);
            if (!string.IsNullOrWhiteSpace(info))
                throw new Exception($"CompileShader {type} had errors: {info}");

            shaders.Add(shader);

            type = ShaderType.FragmentShader;
            shader = GL.CreateShader(type);
            src = @"#version 330 core
                        in vec4 vs_color;
                        in vec3 original_normal;
                        in vec3 transformed_normal;
                        out vec4 color;

                        void main(void)
                        {
                            float lighting = abs(dot(transformed_normal, vec3(0,0,-1)));
                            color = vs_color * lighting;
                        }";
            GL.ShaderSource(shader, src);
            GL.CompileShader(shader);
            info = GL.GetShaderInfoLog(shader);
            if (!string.IsNullOrWhiteSpace(info))
                throw new Exception($"CompileShader {type} had errors: {info}");

            shaders.Add(shader);

            foreach (var shader_ in shaders)
                GL.AttachShader(_program, shader_);
            GL.LinkProgram(_program);
            var info_ = GL.GetProgramInfoLog(_program);
            if (!string.IsNullOrWhiteSpace(info_))
                throw new Exception($"CompileShaders ProgramLinking had errors: {info}");

            foreach (var shader_ in shaders)
            {
                GL.DetachShader(_program, shader_);
                GL.DeleteShader(shader_);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.ToString());
            throw;
        }

        GL.Enable(EnableCap.DepthTest);
        GL.PatchParameter(PatchParameterInt.PatchVertices, 3);
    }

    public void OnClosed()
    {
        GL.DeleteVertexArray(_vertexArray);
        GL.DeleteBuffer(_buffer);
        GL.DeleteProgram(_program);
    }

    public void OnResize(int width, int height)
    {
        _width = width;
        _height = height;

        // Resize the viewport to match the window size.
        GL.Viewport(0, 0, _width, _height);

    }

    private float[] Matrix4ToArray(Matrix4 matrix)
    {
        float[] data = new float[16];
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                data[i * 4 + j] = matrix[i, j];

            }
        }
        return data;
    }

    public void OnRenderFrame()
    {
        var timeStamp = Stopwatch.GetTimestamp();
        _angle += (float)((timeStamp - _lastTimestamp) / (double)_freq);
        _lastTimestamp = timeStamp;

        // Bind the VBO
        GL.BindBuffer(BufferTarget.ArrayBuffer, _buffer);
        // Bind the VAO
        GL.BindVertexArray(_vertexArray);
        // Use/Bind the program
        GL.UseProgram(_program);

        _model = Matrix4.CreateFromAxisAngle(new Vector3(1.0f, 0.0f, 1.0f), _angle);
        _view = Matrix4.LookAt(new Vector3(0.0f, 0.0f, 5.0f), new Vector3(0.0f, 0.0f, 0.0f), Vector3.UnitY);
        _projection = Matrix4.CreatePerspectiveFieldOfView((float)Math.PI * (_FOV / 180f), _width / (float)_height, 0.2f, 256.0f);

        int location = GL.GetUniformLocation(_program, "model");
        GL.UniformMatrix4(location, 1, false, Matrix4ToArray(_model));
        location = GL.GetUniformLocation(_program, "view");
        GL.UniformMatrix4(location, 1, false, Matrix4ToArray(_view));
        location = GL.GetUniformLocation(_program, "projection");
        GL.UniformMatrix4(location, 1, false, Matrix4ToArray(_projection));

        // This draws the triangle.
        GL.DrawArrays(PrimitiveType.Triangles, 0, _verticeCount);
    }
}