# HifeSurvival

## Spec
* Engine : Unity 2021.3.23f1
* IDE : Visual Studio 2019 or VSCode
* Server-Stack : .NET Core 3.1 / C# 8.0


## Plan
- [Meeting / TODO (Notion Docs)](https://kangtae.notion.site/HifeSurvival-baf65c31cf59469a978eec74437163d5?pvs=4)


## Data
- [Game Data](https://docs.google.com/spreadsheets/d/104ZnnXWWorMZOAhuY0o1o1xIL2H41opJlrJLsSEk_C4/edit#gid=0)
- [User Data](https://console.firebase.google.com/u/0/project/planar-hangout-385012/overview?hl=ko)


## Convention
<details markdown="1">
<summary> Code Convention</summary>

### enum

- 접두어 `E` 사용.
- 값은 모두 대문자, `Snake Case`
    
    ```csharp
    public enum EState
    {
        IDLE,
        MOVE,
        SOME_STATE,
    }
    ```
    

### const, readonly

- 이름 모두 대문자, `Snake Case`
    
    ```csharp
    // const
    public const INVALID_VALUE = int.min;
    
    // readonly
    public readonly Vector3 INVALID_POS = new Vector3(-9999,-9999.-9999);
    ```
    

### class

- `Pascal Case`
    
    ```csharp
    public class Item
    {
    
    }
    ```
    
- 변수
    - `Camel Case`
    - 접근 지정자가 public이 아닐 경우, 접두어 `_` 사용.
    ```csharp
    private int _val;
    public int value;
    ```
    
- 프로퍼티
    - `Pascal Case`
    
    ```csharp
    public Vector3 Pos { get; private set;}
    ```
    

### interface

- 접두어 `I` 사용.
- `Pascal Case`
    
    ```csharp
    public interface IState
    {
    
    }
    ```
    

### Collection

- Array
    - 접미어 `Arr` 사용.
    
    ```csharp
    int [] numArr = new int[5];
    ```
    
- List
    - 접미어 `List를` 사용.
    ```csharp
    List<int> numList = new List<int>();
    ```
    
- Dictionary
    - 접미어 `Dict를` 사용.
    ```csharp
    Dictionary<int, int> numDict = new Dictionary<int, int>();
    ```
    

### Function

- `Pascal Case`
- 매개 변수 `CamelCase`
    
    ```csharp
    public void SetHp(int hp)
    {
    
    {
    ```
    
- 반환형 `bool`
    - `Can`, `Has`, `Is` 와 같이 의문문으로 작성.
    
    ```csharp
    public bool IsValied(int value);
    public bool HasValue();
    public bool CanAttack();
    ```
    
- 람다
    - 한 줄일 경우 사용 해도 됨.
    
    ```csharp
    public bool HasValue() => true;
    ```
    

### 이벤트, 액션

- Action<T>
    - 접미어 `Callback` 또는 `CB` 사용.
    ```csharp
    private Action<int> _damageValCallback; 
    ```
    
- Func<T>
    - 접미어 `Func` 사용.
    ```csharp
    private Func<int>  _checkFunc;
    ```
    
- event
    - `On내용Handler` 포맷을 사용.
    ```csharp
    public event Action<T> OnRecvAttackHandler;
    ```
    
### 패킷 구조체

- 접두어 `P` 사용.
    
    ```csharp
    struct PItem
    ```

*****
참고 링크 
> https://docs.popekim.com/ko/coding-standards/pocu-csharp  
> https://learn.microsoft.com/ko-kr/dotnet/csharp/fundamentals/coding-style/coding-conventions

</details>

## Cheat
<details markdown="1">
<summary> Cheat Manual </summary>


아이템 장착 : equipitem [itemKey]


아이템 드롭 : dropitem [itemKey = 1] [itemCount = 1]

</detail>


