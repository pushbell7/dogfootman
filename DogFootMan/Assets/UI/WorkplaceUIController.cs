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
    const float ElementWidth = 30.0f;
    const float ElementHeight = 30.0f;
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

    Vector3 GetDeltaPosition(ETowardDirection direction)
    {
        switch(direction)
        {
            case ETowardDirection.Up: return new Vector3(0, -ElementHeight);
            case ETowardDirection.Left: return new Vector3(-ElementWidth, 0);
            case ETowardDirection.Down: return new Vector3(0, ElementHeight);
            case ETowardDirection.Right: return new Vector3(ElementWidth, 0);
            default: return Vector3.zero;
        }
    }
    VisualElement MakeElement(int row, int col)
    {
        var element = new Button();
        var elementData = SourceItemList[row][col];
        element.text = elementData.Shape.ToString();
        element.userData = elementData;
        element.style.width = ElementWidth;
        element.style.height = ElementHeight;
        SetMargin(element, 0);
        //MakeBorder(element, Color.yellow, 1.0f);
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
                     Stack<ETowardDirection> resultPath;
                     if(IsMatched(element, out resultPath))
                     {
                         // show effect
                         var from = CurrentSelectElement.LocalToWorld(CurrentSelectElement.contentRect.center);
                         var to = element.LocalToWorld(element.contentRect.center);
                         Debug.Log(string.Format("deltaPosition : {0}", to - from));

                         System.Action<MeshGenerationContext> action = (MeshGenerationContext context) =>
                         {
                             List<Vector3> points = new List<Vector3>();
                             points.Add(from);

                             Vector3 currentPosition = from;
                             foreach(var dir in resultPath)
                             {
                                 currentPosition += GetDeltaPosition(dir);
                                 points.Add(currentPosition);
                             }
                             DrawCable(points.ToArray(), 1, Color.red, context);
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
    bool IsMatched(VisualElement other, out Stack<ETowardDirection> outResultPath)
    {
        var selectedElement = (ElementData)CurrentSelectElement.userData;
        var otherElement = (ElementData)other.userData;
        if(selectedElement.Shape != otherElement.Shape)
        {
            outResultPath = null;
            return false;
        }

        outResultPath = GetReachablePath(selectedElement, otherElement);
        return outResultPath != null;
    }

    Stack<ETowardDirection> GetReachablePath(ElementData baseElem, ElementData destElem)
    {
        Stack<ETowardDirection> result = new Stack<ETowardDirection>();
        if(DFSForMatching(baseElem, destElem, 0, ETowardDirection.Max, in result))
        {
            return result;
        }
        return null;
    }

    bool DFSForMatching(ElementData current, ElementData destination, int level, ETowardDirection priorDirection, in Stack<ETowardDirection> inResult)
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
            if (DFSForMatching(GetNext(current, dir), destination, level + (priorDirection == dir ? 0 : 1), dir, inResult))
            {
                inResult.Push(dir);
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
        return y >= 0 && y < Row + 2 && x >= 0 && x < Column + 2;
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

        for (int y = 0; y < Row + 2; ++y)
        {
            var rowList = new List<ElementData>();
            for (int x = 0; x < Column + 2; ++x)
            {
                if (IsBorder(x, y))
                {
                    var border = new ElementData(x, y, -1);
                    border.bIsMatched = true;
                    rowList.Add(border);
                }
                else
                {
                    rowList.Add(new ElementData(x, y, (int)shapes[(y - 1) * Column + (x - 1)]));
                }
            }
            SourceItemList.Add(rowList);
        }
    }
    ElementData GetElement(int x, int y)
    {
        return SourceItemList[y][x];
    }

    bool IsBorder(int x, int y)
    {
        return y == 0 || y == Row + 1 || x == 0 || x == Column + 1;
    }

    void Batch()
    {
        var rowList = new ScrollView(ScrollViewMode.Vertical);
        //MakeBorder(rowList, Color.red, 1.0f);
        rowList.style.paddingBottom = 0;
        rowList.style.paddingTop = 0;
        for (int y = 0; y < Row + 2; ++y)
        {
            var colList = new ScrollView(ScrollViewMode.Horizontal);
            //MakeBorder(colList, Color.blue, 1.0f);
            for (int x = 0; x < Column + 2; ++x)
            {
                colList.Add(MakeElement(y,x));
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
    static void SetMargin(VisualElement element, float margin)
    {
        element.style.marginBottom = margin;
        element.style.marginTop = margin;
        element.style.marginLeft = margin;
        element.style.marginRight = margin;
    }

    static void MakeBorder(VisualElement element, Color color, float width)
    {
        element.style.borderBottomColor = color;
        element.style.borderBottomWidth = width;
        element.style.borderTopColor = color;
        element.style.borderTopWidth = width;
        element.style.borderLeftColor = color;
        element.style.borderLeftWidth = width;
        element.style.borderRightColor = color;
        element.style.borderRightWidth = width;
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
