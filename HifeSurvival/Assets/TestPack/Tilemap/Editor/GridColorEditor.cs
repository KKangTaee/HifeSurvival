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
        // 셀 색상 그리기 구현

        ClearGridCellColors();

        int width = 10; // 또는 원하는 그리드 너비
        int height = 10; // 또는 원하는 그리드 높이

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
                quad.transform.SetParent(grid.transform);

                // Isometric 셀 크기 계산
                float isoCellWidth = grid.cellSize.x * 2;
                float isoCellHeight = grid.cellSize.y * 2;
                quad.transform.localScale = new Vector3(isoCellWidth, isoCellHeight, 1);

                // Isometric 셀 위치 계산
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
        // 셀 색상 지우기 구현
        foreach (Transform child in grid.transform)
        {
            if (child.name.StartsWith("CellColor_"))
            {
                DestroyImmediate(child.gameObject);
            }
        }
    }
}