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

- 앞에 E를 붙임.
- 내용에 들어갈 원소들은 대문자 형태로
    
    ```csharp
    public enum EState
    {
    		IDLE,
    		MOVE,
            SOME_STATE,
    }
    ```
    

### const, readonly

- 대문자로 통일함.
    
    ```csharp
    // const
    public const INVALID_VALUE = int.min;
    
    // readonly
    public readonly Vector3 INVALID_POS = new Vector3(-9999,-9999.-9999);
    ```
    

### class

- 클래스 명 : 대문자
    
    ```csharp
    public class Item
    {
    
    }
    ```
    
- 변수  : private 일 경우 : 이름 시작 지점에 _ (언더바) 를 붙인다
    
    ```csharp
    private int _val;
    ```
    
- 프로퍼티 : 앞글자 대문자 사용
    
    ```csharp
    public Vector3 Pos { get; private set;}
    ```
    

### interface

- I로 시작하며 대문자로 정의한다.
    
    ```csharp
    public interface IState
    {
    
    }
    ```
    

### Collection

- Array 일 경우 : 뒤에 Arr를 붙인다
    
    ```csharp
    int [] numArr = new int[5];
    ```
    
- List 일 경우 : 뒤에 List를 붙인다.
    
    ```csharp
    List<int> numList = new List<int>();
    ```
    
- Dictionary 일 경우 : 뒤에 Dict를 붙임
    
    ```csharp
    Dictionary<int, int> numDict = new Dictionary<int, int>();
    ```
    

### Function

- 첫글자는 대문자로 작성한다
- 파리미터의 네이밍은 in으로 시작
- 기본 함수일 경우 동사형으로 사용함
    
    ```csharp
    public void SetHp(int inHp)
    {
    
    {
    ```
    
- bool 형은 의문형으로 사용함
    
    ```csharp
    public bool IsValied(int inValue);
    public bool HasValue();
    public bool CanAttack();
    ```
    
- (선택) 함수가 한줄일 경우 람다식 메서드 사용
    
    ```csharp
    public bool HasValue() => true;
    ```
    

### 이벤트, 액션

- Action<T> 일 경우 뒤에 Callback 혹은 CB 로 정의
    
    ```csharp
    private Action<int> _damageValCallback; 
    ```
    
- Func<T> 일 경우 뒤에 Func를 붙인다.
    
    ```csharp
    private Func<int>  _checkFunc;
    ```
    
- event는 앞에 `On내용Handler`  로 정의한다.
    
    ```csharp
    public event Action<T> OnRecvAttackHandler;
    ```
    
### 패킷 구조체

- 구조체 명 앞에 P를 붙임.
    
    ```csharp
    struct PItem
    ```

*****
참고 링크 
> https://docs.popekim.com/ko/coding-standards/pocu-csharp  
> https://learn.microsoft.com/ko-kr/dotnet/csharp/fundamentals/coding-style/coding-conventions

</details>

