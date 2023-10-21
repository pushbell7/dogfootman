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
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    VisualElement MakeElement(int row, int col)
    {
        var element = new Button();
        element.text = SourceItemList[row][col].Shape.ToString();
        element.userData = SourceItemList[row][col];
        element.style.width = 30.0f;
        element.style.height = 30.0f;
        element.style.backgroundColor = new StyleColor(Color.white);
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
                         // remove pair
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

    static void SelectElement(VisualElement element)
    {
        element.style.backgroundColor = new StyleColor(Color.gray);
    }

    static void UnselectElement(VisualElement element)
    {
        element.style.backgroundColor = new StyleColor(Color.white);
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
        return DFSForMatching(baseElem, destElem, 0);
    }

    bool DFSForMatching(ElementData current, ElementData destination, int level)
    {
        if (level >= 3) return false;
        if (current == destination) return true;

        // GetToward for 4 direction
        // if it returns null i need to process exceptionally
        // if its not matched, call DFSForMatching from direct prior element


        return false;
    }

    enum ETowardDirection
    {
        Up,
        Down,
        Left, 
        Right,
    };
    ElementData GetToward(ElementData baseElement, ETowardDirection direction)
    {
        int currentX = baseElement.X;
        int currentY = baseElement.Y;

        while(IsInArray(currentX, currentY))
        {
            switch(direction)
            {
                case ETowardDirection.Up: currentY--;break;
                case ETowardDirection.Down: currentY++; break;
                case ETowardDirection.Left: currentX--; break;
                case ETowardDirection.Right: currentX++; break;
            }
            var currentElement = SourceItemList[currentY][currentX];
            if (currentElement.bIsMatched) continue;
            else return currentElement;
        }
        return null;
    }

    bool IsInArray(int x, int y)
    {
        return x >= 0 && x < Column && y >= 0 && y < Row;
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

        for (int i = 0; i < Row; ++i)
        {
            var rowList = new List<ElementData>();
            for (int j = 0; j < Column; ++j)
            {
                rowList.Add(new ElementData(i, j, (int)shapes[i * Column + j]));
            }
            SourceItemList.Add(rowList);
        }
    }

    void Batch()
    {
        var rowList = new ScrollView(ScrollViewMode.Vertical);
        for(int i = 0; i < Row; ++i)
        {
            var colList = new ScrollView(ScrollViewMode.Horizontal);
            for(int j = 0; j < Column; ++j)
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
}
