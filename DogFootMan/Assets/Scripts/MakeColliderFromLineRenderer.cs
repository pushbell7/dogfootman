using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MakeColliderFromLineRenderer : MonoBehaviour
{
    LineRenderer LineRendererComponent;
    // Start is called before the first frame update
    void Start()
    {
        LineRendererComponent = GetComponent<LineRenderer>();
        if (LineRendererComponent)
        {
            var caret = new GameObject();
            caret.transform.rotation = transform.rotation;
            Mesh mesh = new();
            const float HalfWidth = 30.0f / 2;
            List<Vector3> positions = new();
            List<int> triangles = new();
            for(int i = 0; i < LineRendererComponent.positionCount - 1; ++i)
            {
                caret.transform.position = LineRendererComponent.GetPosition(i);
                caret.transform.LookAt(LineRendererComponent.GetPosition(i + 1));
                var left = transform.InverseTransformPoint(caret.transform.position - caret.transform.right * HalfWidth);
                var right = transform.InverseTransformPoint(caret.transform.position + caret.transform.right * HalfWidth);
                positions.Add(left);
                positions.Add(right);

                if (i % 2 == 1)
                {
                    int baseIndex = (i - 1) * 2;
                    triangles.Add(baseIndex);
                    triangles.Add(baseIndex + 2);
                    triangles.Add(baseIndex + 1);

                    triangles.Add(baseIndex + 2);
                    triangles.Add(baseIndex + 1);
                    triangles.Add(baseIndex + 3);
                }
            }
            mesh.vertices = positions.ToArray();
            mesh.triangles = triangles.ToArray();

            MeshCollider collider = gameObject.AddComponent<MeshCollider>();
            collider.sharedMesh = mesh;
        }
    }

    // TODO : export mesh to make collider static component
}
