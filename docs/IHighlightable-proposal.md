# IHighlightable 제안 (미구현)

## 문제
"선택된 것을 강조한다"는 같은 개념이 두 갈래로 구현돼 있다.

| 방식 | 위치 | 대상을 아는 방법 |
|---|---|---|
| 색 틴트 | `MenuIcon.SetHighlighted` + `MenuFocus` | `MenuIcon` 구체 타입 |
| 테두리 스프라이트 | `FocusFrame` | `Rect` 또는 `Renderer` |

`MenuFocus`가 `MenuIcon`을 직접 알아서, 창 위젯이나 HUD 능력 대상에는 같은 "한 번에 하나만 강조, owner가 다르면 덮어쓰지 않음" 규칙을 쓸 수 없다.

## 제안
```csharp
public interface IHighlightable
{
    void SetHighlighted(bool highlighted);
}
```
- `MenuIcon`이 구현한다. 지금의 `SetHighlighted(bool)`과 시그니처가 같아 본문 변경이 없다.
- `MenuFocus`가 `MenuIcon` 대신 `IHighlightable`을 받는다. `Set(owner, target)` / `Release(owner)` 규칙은 그대로다.
- 테두리를 쓰는 대상은 `FocusFrame`을 감싼 작은 어댑터(`FrameHighlight : IHighlightable`)로 묶는다. 생성 시 `FocusFrame`과 대상 `Renderer`를 받고, `SetHighlighted(true)`에서 `Follow(renderer)`, `false`에서 `Hide()`를 부른다.

## 효과
- 타이틀 메뉴(색 틴트)와 HUD 능력 대상(테두리)을 같은 `MenuFocus`로 관리할 수 있다.
- 새 강조 방식(예: 스케일 펄스)을 추가해도 `MenuFocus`를 고치지 않는다.

## 하지 않는 것 / 주의
- `DropdownWidget`의 목록 항목 강조는 대상이 아니다. 위젯 내부 상태(`_highlighted` 인덱스)이고 외부에서 선택하지 않는다.
- 인터페이스에 `Rect`나 `Renderer`를 넣지 않는다. 대상이 자기 강조 방법을 안다는 점이 핵심이다.
- 구현하면 `MenuFocus`, `MenuSelector`, `TitleBootstrap`(생성부) 3곳이 영향을 받는다.
