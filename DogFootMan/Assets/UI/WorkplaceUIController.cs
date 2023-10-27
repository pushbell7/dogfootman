using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class WorkplaceUIController : MonoBehaviour
{
    int Row;
    int Column;
    int ShapeCount;
    VisualElement MainPanel;
    List<List<ElementData>> SourceItemList;
    VisualElement CurrentSelectElement;

    Label TimeLabel;
    float LimitedTime;
    bool bFinished;

    class ElementData
    {
        public int X;
        public int Y;
        public int Shape;
        public bool bIsMatched;

        public ElementData(int x, int y, int shape)
        {
            X = x;
            Y = y;
            Shape = shape;
            bIsMatched = false;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        Row = Random.Range(5, 10);
        Column = Random.Range(5, 10);
        ShapeCount = Random.Range(15, 20);
        if(Row * Column % 2 == 1)
        {
            Column++;
        }

        MainPanel = GetComponent<UIDocument>().rootVisualElement.Q<VisualElement>("MainPanel");
        SourceItemList = new List<List<ElementData>>();

        Init();

        Batch();


        TimeLabel = GetComponent<UIDocument>().rootVisualElement.Q<Label>("Timer");
        LimitedTime = Time.time + 1 * 60;
        bFinished = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (bFinished) return;

        float remainedTime = LimitedTime - Time.time;
        if (remainedTime >= 0)
        {
            TimeLabel.text = string.Format(System.TimeSpan.FromSeconds(remainedTime).ToString(@"mm\:ss"));
        }
        else
        {
            bFinished = true;
            Debug.Log("game over");
        }
    }

    void AddTime(float seconds)
    {
        LimitedTime += seconds;
    }

    VisualElement MakeElement(int row, int col)
    {
        var element = new Button();
        var elementData = SourceItemList[row][col];
        element.text = elementData.Shape.ToString();
        element.userData = elementData;
        element.style.width = 30.0f;
        element.style.height = 30.0f;
        element.style.backgroundColor = new StyleColor(Color.white);
        element.visible = (elementData.bIsMatched == false);
        element.clicked += () =>
         {
             if (CurrentSelectElement == element) // cancel selection
             {
                 UnselectElement(element);
                 CurrentSelectElement = null;
             }
             else
             {
                 if (CurrentSelectElement == null) // first selection
                 {
                     SelectElement(element);
                     CurrentSelectElement = element;
                 }
                 else // second selection
                 {
                     if(IsMatched(element))
                     {
                         // show effect
                         var from = element.LocalToWorld(element.contentRect.center);
                         var to = CurrentSelectElement.LocalToWorld(CurrentSelectElement.contentRect.center);

                         System.Action<MeshGenerationContext> action = (MeshGenerationContext context) =>
                         {
                             Vector3[] points = { from, to };
                             DrawCable(points, 1, Color.red, context);
                         };
                         MainPanel.generateVisualContent += action;
                         MainPanel.MarkDirtyRepaint();

                         UnselectElement(CurrentSelectElement);
                         RemoveElements(CurrentSelectElement, element, action);
                     }
                     else
                     {
                         // unselect pair
                         UnselectElement(element);
                         UnselectElement(CurrentSelectElement);
                     }
                     CurrentSelectElement = null;
                 }
             }
         };
        return element;
    }
    void RemoveElements(VisualElement fromElement, VisualElement toElement, System.Action<MeshGenerationContext> action)
    {
        StartCoroutine(DelayAction(0.3f, ()=> {
            // remove pair
            RemoveElement(fromElement);
            RemoveElement(toElement);

            MainPanel.generateVisualContent -= action;
            MainPanel.MarkDirtyRepaint();
        }));
    }

    delegate void TimerCallback();
    IEnumerator DelayAction(float delayTime, TimerCallback callback)
    {
        yield return new WaitForSeconds(delayTime);

        callback();
    }
    static void SelectElement(VisualElement element)
    {
        element.style.backgroundColor = new StyleColor(Color.gray);
    }

    static void UnselectElement(VisualElement element)
    {
        element.style.backgroundColor = new StyleColor(Color.white);
    }
    static void RemoveElement(VisualElement element)
    {
        var elementData = (ElementData)element.userData;
        elementData.bIsMatched = true;
        element.visible = false;
    }
    bool IsMatched(VisualElement other)
    {
        var selectedElement = (ElementData)CurrentSelectElement.userData;
        var otherElement = (ElementData)other.userData;
        if(selectedElement.Shape != otherElement.Shape)
        {
            return false;
        }
        return IsReachable(selectedElement, otherElement);
    }

    bool IsReachable(ElementData baseElem, ElementData destElem)
    {
        return DFSForMatching(baseElem, destElem, 0, ETowardDirection.Max);
    }

    bool DFSForMatching(ElementData current, ElementData destination, int level, ETowardDirection priorDirection)
    {
        if (level > 3) return false;
        if (current == destination) return true;

        if(current == null || (level != 0 && current.bIsMatched == false))
        {
            return false;
        }

        for(ETowardDirection dir = ETowardDirection.Up; dir < ETowardDirection.Max; ++dir)
        {
            if (IsOpposite(dir, priorDirection)) continue;
            if (DFSForMatching(GetNext(current, dir), destination, level + (priorDirection == dir ? 0 : 1), dir))
            {
                return true;
            }
        }

        return false;
    }

    enum ETowardDirection
    {
        Up,
        Left, 
        Down,
        Right,
        Max
    };
    bool IsOpposite(ETowardDirection one, ETowardDirection other)
    {
        return (ETowardDirection)((int)(one + 2) % (int)ETowardDirection.Max) == other;
    }
    ElementData GetNext(ElementData baseElement, ETowardDirection direction)
    {
        if (baseElement == null) return null;
        int currentX = baseElement.X;
        int currentY = baseElement.Y;

        switch (direction)
        {
            case ETowardDirection.Up: currentY--; break;
            case ETowardDirection.Left: currentX--; break;
            case ETowardDirection.Down: currentY++; break;
            case ETowardDirection.Right: currentX++; break;
        }
        if (IsInArray(currentX, currentY) == false)
        {
            return null;
        }
        return GetElement(currentX, currentY);
    }

    bool IsInArray(int x, int y)
    {
        return x >= 0 && x < Row + 2 && y >= 0 && y < Column + 2;
    }

    void Init()
    {
        ArrayList shapes = new ArrayList();
        int totalCount = Row * Column;
        int halfOfElements = totalCount / 2;
        for (int i = 0; i < halfOfElements; ++i)
        {
            shapes.Add(i % ShapeCount);
            shapes.Add(i % ShapeCount);
        }
        Shuffle(shapes);

        for (int i = 0; i < Row + 2; ++i)
        {
            var rowList = new List<ElementData>();
            for (int j = 0; j < Column + 2; ++j)
            {
                if (IsBorder(i, j))
                {
                    var border = new ElementData(i, j, -1);
                    border.bIsMatched = true;
                    rowList.Add(border);
                }
                else
                {
                    rowList.Add(new ElementData(i, j, (int)shapes[(i - 1) * Column + (j - 1)]));
                }
            }
            SourceItemList.Add(rowList);
        }
    }
    ElementData GetElement(int row, int col)
    {
        return SourceItemList[row][col];
    }

    bool IsBorder(int i, int j)
    {
        return i == 0 || i == Row + 1 || j == 0 || j == Column + 1;
    }

    void Batch()
    {
        var rowList = new ScrollView(ScrollViewMode.Vertical);
        for(int i = 0; i < Row + 2; ++i)
        {
            var colList = new ScrollView(ScrollViewMode.Horizontal);
            for(int j = 0; j < Column + 2; ++j)
            {
                colList.Add(MakeElement(i,j));
            }
            rowList.Add(colList);
        }
        MainPanel.Add(rowList);
    }

    // Shuffle function is generated by Chatgpt
    static void Shuffle(ArrayList array)
    {
        int n = array.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            
            int value = (int)array[k];
            array[k] = array[n];
            array[n] = value;
        }
    }

    // copied from https://forum.unity.com/threads/draw-a-line-from-a-to-b.698618/
    public static void DrawCable(Vector3[] points, float thickness, Color color, MeshGenerationContext context)
    {
        List<Vertex> vertices = new List<Vertex>();
        List<ushort> indices = new List<ushort>();

        for (int i = 0; i < points.Length - 1; i++)
        {
            var pointA = points[i];
            var pointB = points[i + 1];

            float angle = Mathf.Atan2(pointB.y - pointA.y, pointB.x - pointA.x);
            float offsetX = thickness / 2 * Mathf.Sin(angle);
            float offsetY = thickness / 2 * Mathf.Cos(angle);

            vertices.Add(new Vertex()
            {
                position = new Vector3(pointA.x + offsetX, pointA.y - offsetY, Vertex.nearZ),
                tint = color
            });
            vertices.Add(new Vertex()
            {
                position = new Vector3(pointB.x + offsetX, pointB.y - offsetY, Vertex.nearZ),
                tint = color
            });
            vertices.Add(new Vertex()
            {
                position = new Vector3(pointB.x - offsetX, pointB.y + offsetY, Vertex.nearZ),
                tint = color
            });
            vertices.Add(new Vertex()
            {
                position = new Vector3(pointB.x - offsetX, pointB.y + offsetY, Vertex.nearZ),
                tint = color
            });
            vertices.Add(new Vertex()
            {
                position = new Vector3(pointA.x - offsetX, pointA.y + offsetY, Vertex.nearZ),
                tint = color
            });
            vertices.Add(new Vertex()
            {
                position = new Vector3(pointA.x + offsetX, pointA.y - offsetY, Vertex.nearZ),
                tint = color
            });

            ushort indexOffset(int value) => (ushort)(value + (i * 6));
            indices.Add(indexOffset(0));
            indices.Add(indexOffset(1));
            indices.Add(indexOffset(2));
            indices.Add(indexOffset(3));
            indices.Add(indexOffset(4));
            indices.Add(indexOffset(5));
        }

        var mesh = context.Allocate(vertices.Count, indices.Count);
        mesh.SetAllVertices(vertices.ToArray());
        mesh.SetAllIndices(indices.ToArray());
    }
}
