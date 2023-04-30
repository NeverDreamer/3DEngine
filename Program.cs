
using ConsoleGameEngine;
using System;
using System.Diagnostics.Tracing;

public struct vec3d
{
    public vec3d()
    {
        x = 0; y = 0; z = 0;
    }
    public vec3d(float x, float y, float z)
    {
        this.x = x; this.y = y; this.z = z;
    }
    public float x, y, z;
}
public class triangle
{

    public triangle()
    {

    }
    public triangle(vec3d p1,vec3d p2,vec3d p3 )
    {
        this.p3[2] = p3;
        this.p3[1] = p2;
        this.p3[0] = p1;
    }
    public vec3d[] p3 = new vec3d[3] ;
}

public class mesh
{
    public triangle[] triangles = new triangle[12];
}

public class mat4x4
{
    public mat4x4()
    {

    }
    public float[,] m = new float[4,4];
}

class ConsoleApp1 : ConsoleGame
{
    static void Main(string[] args)
    {
        new ConsoleApp1().Construct(63, 63, 2, 2, FramerateMode.Unlimited);
    }
    private mesh meshCube = new mesh();
    private mat4x4 matProj = new mat4x4();
    private float fTheta;
    List<triangle> trianglesToRaster = new List<triangle>();

    void MultiplyMatrixVector(vec3d i, ref vec3d o, mat4x4 m)
    {
        // i - in vector
        // o - out vector
        o.x = i.x * m.m[0,0] + i.y * m.m[1,0] + i.z * m.m[2,0] + m.m[3,0];
        o.y = i.x * m.m[0, 1] + i.y * m.m[1,1] + i.z * m.m[2,1] + m.m[3,1];
        o.z = i.x * m.m[0,2] + i.y * m.m[1,2] + i.z * m.m[2,2] + m.m[3,2];
        float w = i.x * m.m[0,3] + i.y * m.m[1,3] + i.z * m.m[2,3] + m.m[3,3];

        if (w != 0.0f)
        {
            o.x /= w; o.y /= w; o.z /= w;
        }
    }
    public override void Create()
    {
        Engine.SetPalette(Palettes.Pico8);
        Engine.SetBackground(0);
        Engine.Borderless();
        meshCube.triangles[0] = new triangle(new vec3d(0.0f, 0.0f, 0.0f), new vec3d(0.0f, 1.0f, 0.0f), new vec3d(1.0f, 1.0f, 0.0f));
        meshCube.triangles[1] = new triangle(new vec3d(0.0f, 0.0f, 0.0f), new vec3d(1.0f, 1.0f, 0.0f), new vec3d(1.0f, 0.0f, 0.0f));
        meshCube.triangles[2] = new triangle(new vec3d(1.0f, 0.0f, 0.0f), new vec3d(1.0f, 1.0f, 0.0f), new vec3d(1.0f, 1.0f, 1.0f));
        meshCube.triangles[3] = new triangle(new vec3d(1.0f, 0.0f, 0.0f), new vec3d(1.0f, 1.0f, 1.0f), new vec3d(1.0f, 0.0f, 1.0f));
        meshCube.triangles[4] = new triangle(new vec3d(1.0f, 0.0f, 1.0f), new vec3d(1.0f, 1.0f, 1.0f), new vec3d(0.0f, 1.0f, 1.0f));
        meshCube.triangles[5] = new triangle(new vec3d(1.0f, 0.0f, 1.0f), new vec3d(0.0f, 1.0f, 1.0f), new vec3d(0.0f, 0.0f, 1.0f));
        meshCube.triangles[6] = new triangle(new vec3d(0.0f, 0.0f, 1.0f), new vec3d(0.0f, 1.0f, 1.0f), new vec3d(0.0f, 1.0f, 0.0f));
        meshCube.triangles[7] = new triangle(new vec3d(0.0f, 0.0f, 1.0f), new vec3d(0.0f, 1.0f, 0.0f), new vec3d(0.0f, 0.0f, 0.0f));
        meshCube.triangles[8] = new triangle(new vec3d(0.0f, 1.0f, 0.0f), new vec3d(0.0f, 1.0f, 1.0f), new vec3d(1.0f, 1.0f, 1.0f));
        meshCube.triangles[9] = new triangle(new vec3d(0.0f, 1.0f, 0.0f), new vec3d(1.0f, 1.0f, 1.0f), new vec3d(1.0f, 1.0f, 0.0f));
        meshCube.triangles[10] = new triangle(new vec3d(1.0f, 0.0f, 1.0f), new vec3d(0.0f, 0.0f, 1.0f), new vec3d(0.0f, 0.0f, 0.0f));
        meshCube.triangles[11] = new triangle(new vec3d(1.0f, 0.0f, 1.0f), new vec3d(0.0f, 0.0f, 0.0f), new vec3d(1.0f, 0.0f, 0.0f));

        // Projection Matrix
        float fNear = 0.1f;
        float fFar = 1000.0f;
        float fFov = 90.0f;
        float fAspectRatio = (float)Console.WindowHeight / (float)Console.WindowWidth;
        float fFovRad = (float) (1.0f / Math.Tan(fFov * 0.5f / 180.0f * 3.14159f));

        matProj.m[0,0] = fAspectRatio * fFovRad;
        matProj.m[1,1] = fFovRad;
        matProj.m[2,2] = fFar / (fFar - fNear);
        matProj.m[3,2] = (-fFar * fNear) / (fFar - fNear);
        matProj.m[2,3] = 1.0f;
        matProj.m[3,3] = 0.0f;
    }

    public override void Update()
    {
        mat4x4 matRotZ = new mat4x4();
        mat4x4 matRotX = new mat4x4();
        matRotZ.m = new float[4, 4];
        matRotX.m = new float[4, 4];
        fTheta += 0.01f;

        matRotZ.m[0, 0] = (float)Math.Cos(fTheta);
        matRotZ.m[0, 1] = (float)Math.Sin(fTheta);
        matRotZ.m[1, 0] = -1 * (float)Math.Sin(fTheta);
        matRotZ.m[1, 1] = (float)Math.Cos(fTheta);
        matRotZ.m[2, 2] = 1;
        matRotZ.m[3, 3] = 1;

        matRotX.m[0,0] = 1;
        matRotX.m[1,1] = (float)Math.Cos(fTheta * 0.5f);
        matRotX.m[1,2] = (float)Math.Sin(fTheta * 0.5f);
        matRotX.m[2,1] = -1* (float)Math.Sin(fTheta * 0.5f);
        matRotX.m[2,2] = (float)Math.Cos(fTheta * 0.5f);
        matRotX.m[3,3] = 1;

        foreach (triangle tri in meshCube.triangles)
        {
            triangle triProjected, triTranslated, triRotatedZ, triRotatedZX;
            triProjected = new triangle(new vec3d(), new vec3d(), new vec3d());
            triProjected.p3 = new vec3d[3];
            triTranslated = new triangle(new vec3d(), new vec3d(), new vec3d());
            triTranslated.p3 = new vec3d[3];
            triRotatedZ = new triangle(new vec3d(), new vec3d(), new vec3d());
            triRotatedZ.p3 = new vec3d[3];
            triRotatedZX = new triangle(new vec3d(), new vec3d(), new vec3d());
            triRotatedZX.p3 = new vec3d[3];

            MultiplyMatrixVector(tri.p3[0], ref triRotatedZ.p3[0], matRotZ);
            MultiplyMatrixVector(tri.p3[1], ref triRotatedZ.p3[1], matRotZ);
            MultiplyMatrixVector(tri.p3[2], ref triRotatedZ.p3[2], matRotZ);

            MultiplyMatrixVector(triRotatedZ.p3[0], ref triRotatedZX.p3[0], matRotX);
            MultiplyMatrixVector(triRotatedZ.p3[1], ref triRotatedZX.p3[1], matRotX);
            MultiplyMatrixVector(triRotatedZ.p3[2], ref triRotatedZX.p3[2], matRotX);

            triTranslated = triRotatedZX;
            triTranslated.p3[0].z = triRotatedZX.p3[0].z + 3.0f;
            triTranslated.p3[1].z = triRotatedZX.p3[1].z + 3.0f;
            triTranslated.p3[2].z = triRotatedZX.p3[2].z + 3.0f;

            MultiplyMatrixVector(triTranslated.p3[0], ref triProjected.p3[0], matProj);
            MultiplyMatrixVector(triTranslated.p3[1], ref triProjected.p3[1], matProj);
            MultiplyMatrixVector(triTranslated.p3[2], ref triProjected.p3[2], matProj);

            triProjected.p3[0].x += 1.0f; triProjected.p3[0].y += 1.0f;
            triProjected.p3[1].x += 1.0f; triProjected.p3[1].y += 1.0f;
            triProjected.p3[2].x += 1.0f; triProjected.p3[2].y += 1.0f;
            triProjected.p3[0].x *= 0.5f * (float)Console.WindowWidth;
            triProjected.p3[0].y *= 0.5f * (float)Console.WindowHeight;
            triProjected.p3[1].x *= 0.5f * (float)Console.WindowWidth;
            triProjected.p3[1].y *= 0.5f * (float)Console.WindowHeight;
            triProjected.p3[2].x *= 0.5f * (float)Console.WindowWidth;
            triProjected.p3[2].y *= 0.5f * (float)Console.WindowHeight;

            trianglesToRaster.Add(triProjected);

        }
    }

    public override void Render()
    {
        Engine.ClearBuffer();
        foreach (triangle tri in trianglesToRaster)
        {
            

            Engine.FillTriangle(new Point((int)tri.p3[0].x, (int)tri.p3[0].y),
                new Point((int)tri.p3[1].x, (int)tri.p3[1].y),
                new Point((int)tri.p3[2].x, (int)tri.p3[2].y), 2);

        }
        trianglesToRaster.Clear();
        Engine.DisplayBuffer();
    }

}