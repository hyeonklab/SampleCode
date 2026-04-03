# ClosedXML 100만건 엑셀 추출 설계

## 개요

WinForms + .NET 10 + ClosedXML 0.105.0 환경에서 100만건 더미 데이터를 엑셀로 추출하는 기능 구현.

## 기술 선택

- **방식**: ClosedXML `InsertData(IEnumerable)` bulk insert
- **비동기**: `Task.Run` + `IProgress<int>` (UI 블로킹 없음)
- **메모리**: `IEnumerable` yield return으로 스트리밍 생성

## UI 구성

- 저장 경로 텍스트박스 + 찾아보기 버튼 (SaveFileDialog)
- 추출 시작 버튼
- ProgressBar (0~100)
- 상태 레이블 (N / 1,000,000 처리중)
- 완료 시 MessageBox

## 더미 데이터 컬럼 (주문 데이터, 12개)

| 컬럼명 | 타입 |
|--------|------|
| OrderId | int |
| CustomerId | int |
| CustomerName | string |
| Email | string |
| Phone | string |
| ProductName | string |
| Category | string |
| Quantity | int |
| UnitPrice | decimal |
| TotalPrice | decimal |
| OrderDate | DateTime |
| Status | string |

## 추출 흐름

1. 버튼 클릭 → SaveFileDialog로 저장 경로 선택
2. `Task.Run`으로 백그라운드 실행
3. `IEnumerable<object[]>` yield return으로 100만건 생성
4. `ws.Cell(2,1).InsertData(rows)` bulk insert
5. 1만건마다 `IProgress<int>` 로 UI 업데이트
6. `workbook.SaveAs(path)` 저장
7. 완료 MessageBox 표시

## 파일 수정 대상

- `ClosedXML/Form1.Designer.cs` — UI 컨트롤 추가
- `ClosedXML/Form1.cs` — 추출 로직 구현
