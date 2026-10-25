# Poopline — pierwszy grywalny szkic

Platformówka 2D z ręcznym sterowaniem: bohater inspirowany emoji kupy przemierza kanały pod wybranym polskim miastem. Możesz iść w lewo i w prawo, zatrzymać się, zawrócić i skakać. Celem jest dotarcie do oczyszczalni. Bąbelki są opcjonalnymi punktami; nie ma limitu czasu.

To podstawa do sprawdzenia mechaniki, z tymczasową grafiką. Nie przesądza o finalnym bohaterze, stylu ani fabule.

## Uruchomienie w Unity

1. Otwórz ten folder przez Unity Hub w **Unity 6000.5.1f1**.
2. Poczekaj na import i kompilację.
3. Wybierz **Tools → Poopline → Open playable game**.
4. Kliknij **Play**, wybierz miasto na mapie i kliknij **WEJDŹ DO KANAŁÓW** w oknie **Game**. Jeżeli widzisz tylko Scene, otwórz **Window → General → Game**.

Scena startowa: `Assets/Poopline/Scenes/Poopline.unity`. Jest jedyną sceną na liście budowania. Menu „Open playable game” ustawia ją także jako scenę uruchamianą przyciskiem Play.

W trybie edycji scena zawiera obiekt `Poopline`. Poziom, postać, kamera i interfejs powstają po uruchomieniu gry. To celowe: szkic nie wymaga ręcznego podpinania obiektów. Parametry prędkości i skoku są na tym obiekcie w Inspectorze; układ poziomu jest w `SewerWorld.cs`.

## Wybór miasta

Gra zaczyna się od mapy Polski z sześcioma miastami wymienionymi w założeniach: **Mielec, Poznań, Warszawa, Wrocław, Kraków i Gdańsk**. Kliknij znacznik lub nazwę miasta, następnie wejdź do kanałów. Klawiatura i pad pozwalają zmieniać wybór w lewo/prawo i zatwierdzić przyciskiem skoku.

Każde miasto ma kolorystykę, oznaczenie kanału, wariant rozmieszczenia przeszkód i osobny rekord. Na tym etapie sześć wariantów korzysta ze wspólnego układu chodników; nie są odwzorowaniem rzeczywistych sieci kanalizacyjnych. Do mapy można wrócić przyciskiem **MAPA** lub **ZMIEŃ MIASTO**. Powrót kończy bieżącą próbę.

Mapa działa offline, bez konta, API i dostępu do lokalizacji. Źródło uproszczonego obrysu Polski opisano w `MAP_SOURCES.md`. Dodawanie kolejnych miast: `CityCatalog.cs`.

## Sterowanie

| Czynność | Klawiatura | Telefon / mysz | Pad |
| --- | --- | --- | --- |
| Wybór miasta | A/D lub lewo/prawo | znacznik lub nazwa na mapie | drążek lub D-pad |
| Ruch | A/D lub lewo/prawo | przytrzymaj **<** lub **>** | drążek lub D-pad |
| Start / ponów / wznowienie | Spacja lub Enter | przycisk na ekranie | dolny przycisk, np. A |
| Skok | Spacja, W lub strzałka w górę | dotknięcie **SKOK** | dolny przycisk |
| Pauza / wznowienie | Escape | **PAUZA** / **WRACAMY** | Start |
| Od początku | R | przycisk po przegranej lub w pauzie | górny przycisk, np. Y |
| Dźwięk | przycisk na ekranie | przycisk na ekranie | mysz / dotyk |

Postać stoi, gdy nie trzymasz kierunku. Można jednocześnie przytrzymywać kierunek i dotknąć skoku drugim palcem. Puszczenie przycisku, zsunięcie palca, pauza i powrót do mapy zwalniają dotykowe sterowanie. Skok ma krótki bufor wejścia i tolerancję po zejściu z krawędzi. Nie ma podwójnego skoku. Dotknięcie przeszkody lub wpadnięcie do ścieków kończy próbę; restart jest natychmiastowy. Powrót z tła wymaga świadomego wznowienia. Największa liczba bąbelków w ukończonej próbie jest zapisywana oddzielnie dla każdego miasta. Zapisywane są również ostatnie wybrane miasto i ustawienie dźwięku.

## Gdzie pracować

| Miejsce | Zawartość |
| --- | --- |
| `Assets/Poopline/Runtime` | aktualna gra, zasady, fizyka, sterowanie, UI i generowanie poziomu |
| `Assets/Poopline/Scenes` | aktualna scena startowa |
| `Assets/Poopline/Settings` | używane ustawienia renderowania URP |
| `Assets/Poopline/Editor` | otwieranie gry, sprawdzanie sceny i budowanie macOS |
| `Assets/Poopline/Tests/PlayMode` | testy przebiegu gry w silniku Unity |

Cała gra znajduje się w `Assets/Poopline`. Projekt zawiera jedną scenę: `Poopline.unity`, oraz trzy assembly: kod gry, narzędzia edytora i testy. Stare sceny, prototypy, generatory, samouczki i zasoby Platformer Microgame zostały usunięte.

Sterowanie jest definiowane w `RunnerInput.cs`; interfejs korzysta z domyślnych akcji UI pakietu Input System. Nie ma drugiego zestawu akcji odziedziczonego po szablonie. Zależności Cinemachine, Unity Play, samouczków i Multiplayer Center również usunięto. Pozostały pakiety używane przez grę, renderowanie i narzędzia programistyczne Unity.

## Platformy

Kod gry korzysta z Unity Input System i nie wymaga zewnętrznego backendu. Interfejs uwzględnia safe area oraz obrót ekranu. Wspólna scena jest przeznaczona do rozwijania na iOS, Android i desktop.

- **macOS:** `Tools → Poopline → Build macOS prototype` zapisuje lokalną wersję testową w `Builds/macOS/Poopline.app`.
- **iOS:** doinstaluj iOS Build Support dla tej wersji edytora. W `File → Build Profiles` wybierz iOS; Unity wygeneruje projekt Xcode. Przed testem na urządzeniu ustaw własny Bundle Identifier i podpisywanie w Xcode.
- **Android:** doinstaluj Android Build Support wraz z SDK/NDK/OpenJDK, a następnie utwórz profil Android. Ustaw własny identyfikator aplikacji przed przygotowaniem wydania.
- **Windows / Linux:** wymagają odpowiednich modułów i profili budowania. Steam będzie osobnym etapem dystrybucji gotowego wydania desktopowego.

Obecne identyfikatory aplikacji i ustawienia sklepu pochodzą jeszcze z szablonu Unity. Wersja gry to `0.2.0`. Ten szkic nie jest wydaniem przeznaczonym do publikacji w sklepach. Nie dodano reklam, zakupów, logowania, analityki ani integracji Steam.

Dokumentacja Unity: [obsługiwane urządzenia wejścia](https://docs.unity.cn/Packages/com.unity.inputsystem%401.19/manual/SupportedDevices.html), [profile i platformy budowania](https://docs.unity.com/en-us/engine/6000.3/manual/building-and-publishing/build-settings/build-profiles-reference).

## Sprawdzenie i Git

W Unity otwórz `Window → General → Test Runner`, kartę **PlayMode** i uruchom `Poopline.Tests`. Testy obejmują brak automatycznego biegu, zatrzymanie i cofanie, przegraną na przeszkodzie, punkty, restart, pauzę, klawiaturę, pad, jednoczesny dotyk ruchu i skoku, mapę oraz przejście tras wszystkich sześciu miast na rzeczywistej fizyce. Zdarzenie dotykowe w teście nie zastępuje próby na iPhonie lub Androidzie.

Szczegóły wykonanych sprawdzeń i ograniczeń znajdują się w `VALIDATION.md`.

Do repozytorium należą `Assets` wraz z plikami `.meta`, `Packages`, `ProjectSettings` i dokumentacja. `Library`, `Temp`, `Logs`, `UserSettings`, `Builds` oraz lokalne wyniki testów są odtwarzalne i pomijane. Pliki projektów IDE mogą być generowane ponownie przez Unity. Weryfikacja dotyczy bieżącej, oczyszczonej wersji projektu. Nie utworzono commitów.
