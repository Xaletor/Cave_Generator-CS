﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGenerator : MonoBehaviour
{

    public SquareGrid squareGrid;
    public List<Vector3> vertices;
    public List<int> triangles;
    public float wallHeight;
    public MeshFilter walls;

    public Dictionary<int, List<Triangle>> triangleDictionary = new Dictionary<int, List<Triangle>>();
    public List<List<int>> outlines = new List<List<int>>();
    public HashSet<int> checkedVertices = new HashSet<int>();



    public bool displayMeshGizmo;

    public void GenerateMesh(int[,] map, float size, float _wallHeight)
    {
        wallHeight = _wallHeight;
        outlines.Clear();
        checkedVertices.Clear();
        triangleDictionary.Clear();

        squareGrid = new SquareGrid(map, size);

        vertices = new List<Vector3>();
        triangles = new List<int>();
        Mesh mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        for (int x = 0; x < squareGrid.squares.GetLength(0); x++)
        {
            for (int y = 0; y < squareGrid.squares.GetLength(1); y++)
            {
                TriangulateSquare(squareGrid.squares[x, y]);
            }
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();

        Vector2[] uvs = new Vector2[vertices.Count];
        for(int i = 0; i < vertices.Count; i++)
        {
            float percentX = Mathf.InverseLerp(-map.GetLength(0) / 2 * size, map.GetLength(0) / 2 * size, vertices[i].x);
            float percentY = Mathf.InverseLerp(-map.GetLength(1) / 2 * size, map.GetLength(1) / 2 * size, vertices[i].z);
            uvs[i] = new Vector2(percentX, percentY);
            mesh.uv = uvs;
        }

        CreateWallMesh(map);

    }

    public void CreateWallMesh(int[,] map)
    {
        CalculateMeshOutLines();

        List<Vector3> wallVertices = new List<Vector3>();
        List<int> wallTriangles = new List<int>();
        Mesh wallMesh = new Mesh();


        foreach (List<int> outline in outlines)
        {
            for (int i = 0; i < outline.Count - 1; i++)
            {
                int startIndex = wallVertices.Count;
                wallVertices.Add(vertices[outline[i]]); // TopLeft
                wallVertices.Add(vertices[outline[i + 1]]); // TopRight
                wallVertices.Add(vertices[outline[i]] - Vector3.up * wallHeight); // BottomLeft
                wallVertices.Add(vertices[outline[i + 1]] - Vector3.up * wallHeight); // BottomRight

                wallTriangles.Add(startIndex + 0);
                wallTriangles.Add(startIndex + 2);
                wallTriangles.Add(startIndex + 3);

                wallTriangles.Add(startIndex + 3);
                wallTriangles.Add(startIndex + 1);
                wallTriangles.Add(startIndex + 0);
            }
        }
        wallMesh.vertices = wallVertices.ToArray();
        wallMesh.triangles = wallTriangles.ToArray();
        walls.mesh = wallMesh;

        MeshCollider[] m = walls.GetComponents<MeshCollider>();
        for (int i = 0; i < m.Length; i++)
        {
            Destroy(m[i]);
        }

        MeshCollider wallCollider = walls.gameObject.AddComponent<MeshCollider>();
        wallCollider.sharedMesh = wallMesh;
        Vector2[] wuvs = new Vector2[wallMesh.vertices.Length];
        for (int i = 0; i < wallMesh.vertices.Length; i++)
        {
            float x = wallMesh.vertices[i].x;

            if (i + 1 < wallMesh.vertices.Length && x == wallMesh.vertices[i + 1].x && i > 1 && wallMesh.vertices[i - 1].x == x)
            {
                //Fix bug texture
                x = wallMesh.vertices[i].z;

            }
            wuvs[i] = new Vector2(x, wallMesh.vertices[i].y);
            wallMesh.uv = wuvs;
        }


    }

    public void TriangulateSquare(Square square)
    {
        switch (square.configuration)
        {
            case 0:
                {
                    break;
                }
            case 1:
                {
                    MeshFromPoints(square.centreLeft, square.centreBottom, square.bottomLeft);
                    break;
                }
            case 2:
                {
                    MeshFromPoints(square.bottomRight, square.centreBottom, square.centreRight);
                    break;
                }
            case 3:
                {
                    MeshFromPoints(square.centreRight, square.bottomRight, square.bottomLeft, square.centreLeft);
                    break;
                }
            case 4:
                {
                    MeshFromPoints(square.topRight, square.centreRight, square.centreTop);
                    break;
                }
            case 5:
                {
                    MeshFromPoints(square.centreTop, square.topRight, square.centreRight, square.centreBottom, square.bottomLeft, square.centreLeft);
                    break;
                }
            case 6:
                {
                    MeshFromPoints(square.centreTop, square.topRight, square.bottomRight, square.centreBottom);
                    break;
                }
            case 7:
                {
                    MeshFromPoints(square.centreTop, square.topRight, square.bottomRight, square.bottomLeft, square.centreLeft);
                    break;
                }
            case 8:
                {
                    MeshFromPoints(square.topLeft, square.centreTop, square.centreLeft);
                    break;
                }
            case 9:
                {
                    MeshFromPoints(square.topLeft, square.centreTop, square.centreBottom, square.bottomLeft);
                    break;
                }
            case 10:
                {
                    MeshFromPoints(square.topLeft, square.centreTop, square.centreRight, square.bottomRight, square.centreBottom, square.centreLeft);
                    break;
                }
            case 11:
                {
                    MeshFromPoints(square.topLeft, square.centreTop, square.centreRight, square.bottomRight, square.bottomLeft);
                    break;
                }
            case 12:
                {
                    MeshFromPoints(square.topLeft, square.topRight, square.centreRight, square.centreLeft);
                    break;
                }
            case 13:
                {
                    MeshFromPoints(square.topLeft, square.topRight, square.centreRight, square.centreBottom, square.bottomLeft);
                    break;
                }
            case 14:
                {
                    MeshFromPoints(square.topLeft, square.topRight, square.bottomRight, square.centreBottom, square.centreLeft);
                    break;
                }
            case 15:
                {
                    MeshFromPoints(square.topLeft, square.topRight, square.bottomRight, square.bottomLeft);
                    checkedVertices.Add(square.topLeft.vertIndex);
                    checkedVertices.Add(square.topRight.vertIndex);
                    checkedVertices.Add(square.bottomRight.vertIndex);
                    checkedVertices.Add(square.bottomLeft.vertIndex);
                    break;
                }
        }
    }

    public void MeshFromPoints(params Node[] points)
    {
        AssignVertices(points);
        if (points.Length >= 3)
        {
            CreateTriangle(points[0], points[1], points[2]);
        }
        if (points.Length >= 4)
        {
            CreateTriangle(points[0], points[2], points[3]);
        }
        if (points.Length >= 5)
        {
            CreateTriangle(points[0], points[3], points[4]);
        }
        if (points.Length >= 6)
        {
            CreateTriangle(points[0], points[4], points[5]);
        }
    }

    public void AssignVertices(Node[] points)
    {
        for (int i = 0; i < points.Length; i++)
        {
            if (points[i].vertIndex == -1)
            {
                points[i].vertIndex = vertices.Count;
                vertices.Add(points[i].position);
            }
        }
    }

    public struct Triangle
    {
        public int vertexIndexA;
        public int vertexIndexB;
        public int vertexIndexC;

        public int[] vertices;

        public Triangle(int a, int b, int c)
        {
            vertices = new int[3];

            vertexIndexA = a;
            vertexIndexB = b;
            vertexIndexC = c;

            vertices[0] = a;
            vertices[1] = b;
            vertices[2] = c;

        }

        public int this[int i]
        {
            get
            {
                return vertices[i];
            }
        }

        public bool Contains(int vertexIndex)
        {
            return vertexIndex == vertexIndexA || vertexIndex == vertexIndexB || vertexIndex == vertexIndexC;
        }
    }

    public void CreateTriangle(Node a, Node b, Node c)
    {
        triangles.Add(a.vertIndex);
        triangles.Add(b.vertIndex);
        triangles.Add(c.vertIndex);

        Triangle triangle = new Triangle(a.vertIndex, b.vertIndex, c.vertIndex);
        AddTriangleToDictionary(triangle.vertexIndexA, triangle);
        AddTriangleToDictionary(triangle.vertexIndexB, triangle);
        AddTriangleToDictionary(triangle.vertexIndexC, triangle);
    }

    public void AddTriangleToDictionary(int vertexIndexKey, Triangle triangle)
    {
        if (triangleDictionary.ContainsKey(vertexIndexKey))
        {
            triangleDictionary[vertexIndexKey].Add(triangle);
        }
        else
        {
            List<Triangle> triangleList = new List<Triangle>();
            triangleList.Add(triangle);
            triangleDictionary.Add(vertexIndexKey, triangleList);
        }
    }

    public bool IsOutlineEdge(int vertexA, int vertexB)
    {
        List<Triangle> trianglesContainingVertexA = triangleDictionary[vertexA];
        int sharedTriangleCount = 0;
        for (int i = 0; i < trianglesContainingVertexA.Count; i++)
        {
            if (trianglesContainingVertexA[i].Contains(vertexB))
            {
                sharedTriangleCount++;
                if (sharedTriangleCount > 1)
                {
                    break;
                }
            }
        }
        return sharedTriangleCount == 1;
    }

    public int GetConnectedOutlineVertex(int vertexIndex)
    {
        List<Triangle> trianglesContainingVertex = triangleDictionary[vertexIndex];

        for (int i = 0; i < trianglesContainingVertex.Count; i++)
        {
            Triangle triangle = trianglesContainingVertex[i];
            for (int j = 0; j < 3; j++)
            {
                int vertexB = triangle[j];
                if (vertexB != vertexIndex && !checkedVertices.Contains(vertexB))
                {
                    if (IsOutlineEdge(vertexIndex, vertexB))
                    {
                        return vertexB;
                    }
                }
            }
        }
        return -1;
    }

    public void CalculateMeshOutLines()
    {
        for (int vertexIndex = 0; vertexIndex < vertices.Count; vertexIndex++)
        {
            if (!checkedVertices.Contains(vertexIndex))
            {
                int newOutlineVertex = GetConnectedOutlineVertex(vertexIndex);
                if (newOutlineVertex != -1)
                {
                    checkedVertices.Add(vertexIndex);
                    List<int> newOutline = new List<int>();
                    newOutline.Add(vertexIndex);
                    outlines.Add(newOutline);
                    FollowOutline(newOutlineVertex, outlines.Count - 1);
                    outlines[outlines.Count - 1].Add(vertexIndex);
                }
            }
        }
    }

    public void FollowOutline(int vertexIndex, int outlineIndex)
    {
        outlines[outlineIndex].Add(vertexIndex);
        checkedVertices.Add(vertexIndex);
        int nextVertexIndex = GetConnectedOutlineVertex(vertexIndex);
        if (nextVertexIndex != -1)
        {
            FollowOutline(nextVertexIndex, outlineIndex);
        }
    }

    public class Node
    {
        public int vertIndex = -1;
        public Vector3 position;

        public Node(Vector3 _pos)
        {
            position = _pos;
        }
    }

    public class ControlNode : Node
    {
        public bool active;
        public Node above, right;

        public ControlNode(Vector3 _pos, bool _active, float squareSize) : base(_pos)
        {
            active = _active;
            above = new Node(position + Vector3.forward * squareSize / 2f);
            right = new Node(position + Vector3.right * squareSize / 2f);

        }
    }

    public class Square
    {
        public ControlNode topLeft, topRight, bottomRight, bottomLeft;
        public Node centreTop, centreRight, centreBottom, centreLeft;
        public int configuration;

        public Square(ControlNode _topLeft, ControlNode _topRight, ControlNode _bottomRight, ControlNode _bottomLeft)
        {
            topLeft = _topLeft;
            topRight = _topRight;
            bottomRight = _bottomRight;
            bottomLeft = _bottomLeft;

            centreTop = topLeft.right;
            centreRight = bottomRight.above;
            centreBottom = bottomLeft.right;
            centreLeft = bottomLeft.above;

            if (topLeft.active)
            {
                configuration += 8;
            }
            if (topRight.active)
            {
                configuration += 4;
            }
            if (bottomRight.active)
            {
                configuration += 2;
            }
            if (bottomLeft.active)
            {
                configuration += 1;
            }
        }
    }

    public class SquareGrid
    {
        public Square[,] squares;

        public SquareGrid(int[,] map, float size)
        {
            int nodeCountX = map.GetLength(0);
            int nodeCountY = map.GetLength(1);

            float mapWidth = nodeCountX * size;
            float mapHeight = nodeCountY * size;

            ControlNode[,] controlNodes = new ControlNode[nodeCountX, nodeCountY];

            for (int x = 0; x < nodeCountX; x++)
            {
                for (int y = 0; y < nodeCountY; y++)
                {
                    Vector3 pos = new Vector3(-mapWidth / 2 + x * size + size / 2, 0, -mapHeight / 2 + y * size + size / 2);
                    controlNodes[x, y] = new ControlNode(pos, map[x, y] == 1, size);
                }
            }

            squares = new Square[nodeCountX - 1, nodeCountY - 1];
            for (int x = 0; x < nodeCountX - 1; x++)
            {
                for (int y = 0; y < nodeCountY - 1; y++)
                {
                    squares[x, y] = new Square(controlNodes[x, y + 1], controlNodes[x + 1, y + 1], controlNodes[x + 1, y], controlNodes[x, y]);
                }
            }
        }
    }


    public void OnDrawGizmos()
    {
        if (displayMeshGizmo)
        {
            if (squareGrid != null)
            {
                for (int x = 0; x < squareGrid.squares.GetLength(0); x++)
                {
                    for (int y = 0; y < squareGrid.squares.GetLength(1); y++)
                    {
                        Gizmos.color = (squareGrid.squares[x, y].topLeft.active) ? Color.black : Color.white;
                        Gizmos.DrawCube(squareGrid.squares[x, y].topLeft.position, Vector3.one * 0.4f);

                        Gizmos.color = (squareGrid.squares[x, y].topRight.active) ? Color.black : Color.white;
                        Gizmos.DrawCube(squareGrid.squares[x, y].topRight.position, Vector3.one * 0.4f);

                        Gizmos.color = (squareGrid.squares[x, y].bottomRight.active) ? Color.black : Color.white;
                        Gizmos.DrawCube(squareGrid.squares[x, y].bottomRight.position, Vector3.one * 0.4f);

                        Gizmos.color = (squareGrid.squares[x, y].bottomLeft.active) ? Color.black : Color.white;
                        Gizmos.DrawCube(squareGrid.squares[x, y].bottomLeft.position, Vector3.one * 0.4f);

                        Gizmos.color = Color.gray;

                        Gizmos.DrawCube(squareGrid.squares[x, y].centreTop.position, Vector3.one * 0.15f);
                        Gizmos.DrawCube(squareGrid.squares[x, y].centreRight.position, Vector3.one * 0.15f);
                        Gizmos.DrawCube(squareGrid.squares[x, y].centreBottom.position, Vector3.one * 0.15f);
                        Gizmos.DrawCube(squareGrid.squares[x, y].centreLeft.position, Vector3.one * 0.15f);

                    }
                }
            }
        }

    }
}
