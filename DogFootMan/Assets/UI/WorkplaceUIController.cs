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

    List<List<int>> SourceItemList;
    VisualElement CurrentSelectElement;
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
        SourceItemList = new List<List<int>>();

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
        element.text = SourceItemList[row][col].ToString();
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
        if((int)CurrentSelectElement.userData != (int)other.userData)
        {
            return false;
        }
        // find path. if exist return true or false
        return false;
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
            var rowList = new List<int>();
            for (int j = 0; j < Column; ++j)
            {
                rowList.Add((int)shapes[i * Column + j]);
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
