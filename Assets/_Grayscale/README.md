# Selective Grayscale (URP)

Пост-эффект для Unity 6 / URP: весь экран переводится в оттенки серого, но пиксели с выбранными оттенками (hue) остаются цветными. Подходит для крови, глаз, одежды и любых объектов — без отдельных материалов на каждый меш: решение смотрит на **итоговый цвет пикселя кадра**.

Тестовая сцена: `Grayscale.unity`.

---

## Идея в одном предложении

После того как камера уже отрисовала сцену, URP делает fullscreen blit: читает картинку экрана, для каждого пикселя считает «насколько он похож по оттенку на список сохранённых цветов» и либо оставляет цвет, либо заменяет на серый.

```
Сцена → Opaque/Transparent/Post → [Full Screen Pass] → экран
                                      ↑
                         материал Hidden/SelectiveGrayscale
                         параметры из SelectiveGrayscaleController
```

---

## Состав папки

| Файл | Роль |
|------|------|
| `Grayscale.shader` | Fullscreen blit-шейдер: читает `_BlitTexture`, вызывает логику из HLSL |
| `Grayscale.hlsl` | Алгоритм: luminance, hue, маска «оставить цвет», сила эффекта |
| `GrayscaleMat.mat` | Материал на шейдере `Hidden/SelectiveGrayscale` — его вешает Full Screen Pass |
| `SelectiveGrayscaleControlle.cs` | Пишет массив цветов и параметры в материал; тогглит feature |
| `New Universal Render Pipeline Asset.asset` | URP Asset (в Graphics Settings должен быть он или его копия) |
| `New Universal Render Pipeline Asset_Renderer.asset` | Universal Renderer + **Full Screen Pass Renderer Feature** |
| `Grayscale.unity` | Тестовая сцена с кубами и контроллером |
| `TestColor1*.mat` | URP/Lit материалы для проверки (жёлтый / зелёный / красный) |

---

## Как кадр проходит через эффект

### 1. Обычный рендер URP

Камера (тип **Base**, Renderer = этот Universal Renderer) рисует геометрию, прозрачность, при необходимости пост-обработку.

### 2. Full Screen Pass Renderer Feature

На asset'е renderer'а добавлен feature:

- **Pass Material** → `GrayscaleMat`
- **Fetch Color Buffer** → включён (копия текущего цвета кадра как источник)
- **Requirements** → **None** (важно: не Color — Color = `_CameraOpaqueTexture`, это другой буфер)
- **Injection Point** → `After Rendering Post Processing` (значение 600)

Feature в конце кадра:

1. Копирует active color в промежуточную текстуру.
2. Рисует fullscreen-треугольник (`DrawProcedural` / blit) материалом эффекта.
3. Записывает результат обратно в цвет камеры.

URP сам биндит `_BlitTexture` и `_BlitScaleBias`. Вершинный шейдер **не пишем сами** — он приходит из `Blit.hlsl` (`Vert`).

### 3. Фрагментный шейдер

`Grayscale.shader`:

1. Сэмплит экран: `SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, uv, ...)`.
2. Вызывает `ApplySelectiveGrayscale(col)` из `Grayscale.hlsl`.
3. Возвращает итоговый RGB.

### 4. Контроллер параметров

`SelectiveGrayscaleController` каждый кадр (и в Edit Mode через `[ExecuteAlways]`) записывает в **тот же** материал:

- `_KeepColors[16]` — эталонные цвета
- `_KeepColorCount` — сколько из массива использовать
- `_MinSat`, `_MinVal`, `_Softness`
- `_EffectAmount` — 1 = эффект вкл, 0 = выкл (на случай если pass всё ещё крутится)

Плюс, если задан `rendererData`, находит `FullScreenPassRendererFeature` с этим материалом и вызывает `SetActive(effectEnabled)` — тогда pass **вообще не ставится в очередь** (дешевле, чем blit с amount=0).

---

## Алгоритм в `Grayscale.hlsl`

Для каждого пикселя цвета `col`:

### Шаг A — серая версия

```
gray = dot(col, float3(0.2126, 0.7152, 0.0722))  // Rec.709 luminance
```

### Шаг B — маска «сохранить цвет» (`SG_KeepMask`)

1. Считаются saturation и value (яркость max-канала).
2. Если `sat < _MinSat` или `value < _MinVal` → маска 0  
   (почти серые и слишком тёмные пиксели не «спасаем» — иначе шум/грязь остаются цветными).
3. Считается **hue** пикселя в диапазоне `[0..1]` (круг оттенков).
4. Для каждого эталона из `_KeepColors[i]`:
   - hue эталона;
   - кратчайшая дистанция по кругу hue (`SG_HueDist`);
   - вес через `smoothstep` с шириной из **alpha** цвета (`_KeepColors[i].a`) и `_Softness`.
5. Итоговая маска = **max** по всем эталонам (если пиксель близок хотя бы к одному — оставляем).

Hue ориентиры примерно:

| Цвет | Hue ≈ |
|------|-------|
| Красный | 0.00 |
| Зелёный | 0.33 |
| Синий | 0.66 |

### Шаг C — смешивание

```
selective = lerp(gray, col, keep)           // серый ↔ оригинал по маске
result    = lerp(col, selective, _EffectAmount)  // выключатель эффекта
```

Почему **hue**, а не сравнение RGB «один в один»:

- кровь бывает светлее/темнее, но оттенок тот же;
- платье в тени остаётся «синим» по hue;
- один эталон покрывает целую полосу яркости.

Цена: **любой** пиксель с похожим hue (красная стена, красный UI) тоже останется цветным. Для точечного контроля нужны маски/слои — это уже другой подход.

---

## Массив цветов: формат

В Inspector у контроллера `keepColors[]`:

| Канал | Смысл |
|--------|--------|
| **R, G, B** | Эталонный цвет (пипеткой с текстуры/экрана) |
| **A** | Ширина допуска по hue (~`0.03`–`0.12`). Уже → только узкий оттенок; шире → больше вариаций |

Лимит: **16** цветов (`MAX_KEEP_COLORS`). Unity передаёт массив фиксированной длины через `Material.SetVectorArray`.

Параметры фильтра:

| Поле | Назначение |
|------|------------|
| `minSaturation` | Ниже — пиксель считается «серым» и не сохраняется |
| `minValue` | Слишком тёмные не сохраняются |
| `softness` | 0 = жёсткий край маски, 1 = мягкий переход к серому |

---

## Тоггл включения / выключения

### В Inspector

Чекбокс **Effect Enabled** на `SelectiveGrayscaleController`.

### Из кода

```csharp
controller.SetEffectEnabled(true);  // или false
```

### Что происходит при выключении

1. `FullScreenPassRendererFeature.SetActive(false)` — pass не выполняется (нужен заполненный `rendererData`).
2. `_EffectAmount = 0` — если pass всё же выполнится, шейдер вернёт исходный цвет без изменений.

При включении — обратное: `SetActive(true)` и `_EffectAmount = 1`.

---

## Настройка с нуля (чеклист)

1. **URP в проекте**  
   `Edit → Project Settings → Graphics` → Scriptable Render Pipeline Settings = ваш URP Asset (здесь: `New Universal Render Pipeline Asset`).

2. **Renderer Feature**  
   Открыть `…_Renderer.asset` → Add Renderer Feature → **Full Screen Pass**:
   - Pass Material = `GrayscaleMat`
   - Fetch Color Buffer = On
   - Requirements = **None**
   - Injection Point = After Rendering Post Processing

3. **Материал**  
   Shader = `Hidden/SelectiveGrayscale` (не URP/Lit).

4. **Камера**  
   - Render Type = Base  
   - Renderer = этот же Universal Renderer (или Default = −1, если он один в списке)  
   - Post Processing по желанию  

5. **Контроллер в сцене**  
   - `effectMaterial` = `GrayscaleMat`  
   - `rendererData` = тот же `…_Renderer.asset`  
   - заполнить `keepColors`

6. **Материалы объектов**  
   В URP используйте **URP/Lit** (или другие URP-шейдеры), не Built-in Standard — иначе розовые/неверные цвета.

---

## Почему `.shader` и `.hlsl` разделены

- **`.shader`** — точка входа Unity: Pass, includes URP/Core/`Blit.hlsl`, вершина `Vert`, сэмплинг `_BlitTexture`.
- **`.hlsl`** — чистая математика без привязки к blit; проще править и переиспользовать (например, из Shader Graph Custom Function).

Имена вроде `Varyings`, `_BlitTexture`, `SAMPLE_TEXTURE2D_X` появляются **только** после:

```hlsl
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
```

В голом `.hlsl` их нет — и не должно быть.

---

## LateUpdate vs OnRenderImage

| API | Когда использовать |
|-----|-------------------|
| `LateUpdate` / `Push()` | Только **запись параметров** в материал |
| Full Screen Pass / Renderer Feature | Сам **эффект на кадр** в URP |
| `OnRenderImage` | Только **Built-in** RP; в URP **не вызывается** |

Этот проект на URP → эффект через Feature, параметры через контроллер.

---

## Типичные проблемы

| Симптом | Что проверить |
|---------|----------------|
| Эффект вообще не виден | Graphics → правильный URP Asset; Feature Active; Fetch Color Buffer; материал не розовый |
| Розовый экран / материал | Ошибка компиляции шейдера — Console |
| Всё цветное, feature «включён» | Requirements был Color — должен быть **None**; камера на другом Renderer |
| Кубики розовые | Материалы не URP/Lit |
| Нужный цвет тоже сереет | Добавить эталон в `keepColors`, увеличить A (ширину), снизить `minSaturation` / `minValue` |
| Лишний цвет остаётся (стена и т.п.) | Сузить A, поднять `minSaturation`, подобрать эталон точнее пипеткой |
| Тоггл не гасит pass | Заполнить `rendererData` и убедиться, что Pass Material = тот же `effectMaterial` |
| UI тоже сереет | Overlay-камера / отдельный Renderer без этого Feature, либо UI после эффекта |

---

## Ограничения подхода

1. Работает по **финальному цвету кадра** (после освещения, тумана, пост-FX до injection point). Hue «крови» на экране может отличаться от albedo в текстуре — эталоны лучше брать пипеткой из Game view.
2. Максимум 16 эталонов.
3. Не отличает «кровь» от «красной бочки» с тем же hue — только цвет, не семантика объекта.
4. Bloom / сильное размытие до pass размазывает цветные края; injection после пост-процесса обычно предсказуее.

---

## Поток данных (схема)

```
keepColors[] ──┐
minSat/Val ────┤  SelectiveGrayscaleController.Push()
softness ──────┤           │
effectEnabled ─┤           ▼
               │    GrayscaleMat (GPU uniforms)
               │           │
rendererData ──┴──► FullScreenPass.SetActive()
                           │
                           ▼
              URP Frame: copy color → Frag(Grayscale.shader)
                           │
                           ▼
                    Grayscale.hlsl
                    ApplySelectiveGrayscale()
                           │
                           ▼
                      Camera color (экран)
```

---

## Быстрый тест в `Grayscale.unity`

1. Play.
2. При `Effect Enabled` = on: фон/жёлтый куб серые; красный и зелёный — цветные (если эталоны совпадают).
3. Снять **Effect Enabled** — полная цветная картинка, без grayscale pass.

Из кода геймплея:

```csharp
[SerializeField] SelectiveGrayscaleController grayscale;

void OnAbilityStart() => grayscale.SetEffectEnabled(true);
void OnAbilityEnd()   => grayscale.SetEffectEnabled(false);
```
