using UnityEngine;
using UnityEditor;

// [CustomEditor(typeof(Grid))]
public class GridColorEditor : Editor
{
    private Grid grid;
    private Color cellColor = Color.red;

    private Editor originalEditor;

    private void OnEnable()
    {
        grid = (Grid)target;
        originalEditor = CreateEditor(grid, System.Type.GetType("UnityEditor.GridEditor, UnityEditor"));
    }

    private void OnDisable()
    {
        if (originalEditor != null)
        {
            DestroyImmediate(originalEditor);
        }
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        cellColor = EditorGUILayout.ColorField("Cell Color", cellColor);

        if (GUILayout.Button("Draw Cell Color"))
        {
            DrawGridCellColors();
        }

        if (GUILayout.Button("Clear Cell Color"))
        {
            ClearGridCellColors();
        }
    }

    private void DrawGridCellColors()
    {
        // �� ���� �׸��� ����

        ClearGridCellColors();

        int width = 10; // �Ǵ� ���ϴ� �׸��� �ʺ�
        int height = 10; // �Ǵ� ���ϴ� �׸��� ����

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
                quad.transform.SetParent(grid.transform);

                // Isometric �� ũ�� ���
                float isoCellWidth = grid.cellSize.x * 2;
                float isoCellHeight = grid.cellSize.y * 2;
                quad.transform.localScale = new Vector3(isoCellWidth, isoCellHeight, 1);

                // Isometric �� ��ġ ���
                Vector3 cellPosition = new Vector3(
                    x * grid.cellSize.x / 2 + y * grid.cellSize.x / 2,
                    x * grid.cellSize.y / 2 - y * grid.cellSize.y / 2,
                    0
                );

                quad.transform.position = cellPosition;

                Material material = new Material(Shader.Find("Unlit/Color"));
                material.color = cellColor;
                quad.GetComponent<MeshRenderer>().material = material;

                quad.name = $"CellColor_{x}_{y}";
                quad.hideFlags = HideFlags.HideInHierarchy;
            }
        }
    }

    private void ClearGridCellColors()
    {
        // �� ���� ����� ����
        foreach (Transform child in grid.transform)
        {
            if (child.name.StartsWith("CellColor_"))
            {
                DestroyImmediate(child.gameObject);
            }
        }
    }
}