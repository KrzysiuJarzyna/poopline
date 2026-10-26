# Poopline 0.4 — dwa etapy pod miastem

Grywalna przygodowa gra 3D ze stałą kamerą. Człowiek z telefonem przechodzi przez stare miejskie kanały, brodzi w nurcie i szuka drogi między pomostami. Polski klimat podziemi kamienic: ceglane sklepienia, betonowe płyty, żeliwne kraty, rury ciepłownicze i ciepłe lampy w mroku. Nie ma walki ani swobodnego skoku.

## Uruchomienie

1. Otwórz projekt w Unity Hub, używając **Unity 6000.5.1f1**.
2. Wybierz **Tools → Poopline → Open playable canal**.
3. Kliknij **Play**, a następnie okno **Game**, żeby kierować postacią.

Gra zaczyna pierwszy etap od razu. Przejście przez końcowy boczny kanał uruchamia drugi etap po krótkim wygaszeniu obrazu. Drugi kończy się ekranem ukończenia obu poziomów. Nie ma trzeciego etapu.

## Poziomy

**01 — Pod kamienicami.** Spokojne poznawanie chodzenia, telefonu, brodzenia, nurtu i wychodzenia z wody. Cztery odcinki kolektora połączone wąskimi kanałami, otwarte komory boczne, przecieki i niskie mostki. Zamknięte fragmenty chodnika zachęcają do wejścia do wody. Bez przeciwników. Projektowane na około trzy minuty pierwszego przejścia; eksploracja i tempo gracza zmieniają czas.

**02 — Żeliwne mosty.** Dłuższa droga z trzema górnymi obejściami, sześcioma drabinami i pięcioma patrolującymi szczurami. Można wybierać drogę górą, wodą lub wolnym chodnikiem. Boczne odnogi, galerie pomp i zmiany kierunku utrudniają orientację. Kontakt ze szczurem odsyła do ostatniego punktu kontrolnego w bieżącym etapie.

Każdy poziom ma trzy punkty kontrolne przy wejściach do kolejnych odcinków. Nie trzeba ich ręcznie aktywować. Stan nie jest zapisywany po zamknięciu gry.

## Sterowanie

| Czynność | Klawisze |
| --- | --- |
| Chodzenie / brodzenie | WASD lub strzałki |
| Drabina ustawiona przodem — wejście / zejście | W/↑ lub S/↓ |
| Drabina boczna — wejście / zejście | A/← lub D/→ |
| Zatrzymanie na drabinie | Puść kierunek; przeciwny pozwala zawrócić |
| Pauza / wznowienie | Escape |
| Restart aktualnego etapu | R |

Drabina chwyta automatycznie przy ruchu zgodnym z jej ustawieniem; nie używa E. Podpowiedź w niebieskim panelu pokazuje właściwy kierunek. Po dotarciu na podest zwolnij kierunek i naciśnij go ponownie, żeby ruszyć dalej.

**Z górnych pomostów schodzi się wyłącznie drabinami.** Widoczne poręcze mają kolizje, a kontroler zabezpiecza także krawędzie otworów przy drabinach. Na dolnym chodniku można swobodnie wchodzić do wody i wychodzić na oba brzegi. Nurt znosi postać, ale można iść pod prąd. Górne pomosty pozostają suche.

Utrata fokusu zatrzymuje rozgrywkę. Powrót do okna nie wznawia jej automatycznie. R restartuje bieżący poziom; przy ekranie ukończenia zaczyna całą podróż od pierwszego.

## Światło i woda

Scenę oświetlają miejscowe ciepłe lampy oraz chłodny telefon, obracany wraz z postacią. Zachowane są cienie i delikatne odbicie światła pozwalające widzieć bohatera. Nie ma słońca, światła otoczenia ani ogólnego rozjaśnienia kanału. Dalsze odcinki wyłaniają się z mroku po podejściu, również podczas ruchu w bocznych tunelach. Nie ma trwałej pamięci odkrytej mapy. Sklepienia i nadproża zasłaniające postać są chwilowo odsłaniane dla kamery; zachowują kolizje i cienie.

Woda przechodzi przez murowane wloty i odpływy za żeliwnymi kratami. Za nimi jest dalszy odcinek wody i spadek do ciemnego kolektora. Boczne kanały są dostępne dla postaci; widać w nich krople z przewodów i kratowane odpływy w podłodze. To animowana geometria i lokalne prądy, bez symulacji hydraulicznej. Instrukcje pozostają wyłącznie w niebieskim interfejsie — w świecie nie ma napisów ani etykiet.

## Edycja

Scena `Assets/Poopline/Scenes/Poopline.unity` zawiera dwa zwykłe, edytowalne obiekty etapów: `01 — Pod kamienicami` i `02 — Żeliwne mosty`. Pierwszy jest aktywny przy starcie; drugi włącza się po ukończeniu pierwszego. Postać, kamera i interfejs są wspólne. Aby obejrzeć drugi etap przed Play, można tymczasowo przełączyć aktywność obu obiektów w Inspectorze. Play zawsze rozpoczyna od pierwszego etapu.

**Rebuild playable canal** odtwarza oba układy z przepisu i zastępuje ręczne zmiany sceny. Nie jest potrzebne do zwykłego uruchamiania. Kopia układu sprzed rozbudowy znajduje się lokalnie w `Library/PooplineBackups/before-two-levels-20261026.tar.gz`.

- `Runtime/PooplineGame.cs`, `SewerLevel.cs` — kolejność etapów, przejścia, punkty kontrolne i restart.
- `Runtime/SewerPlayer.cs`, `ElevatedWalkway.cs` — ruch, brodzenie, wspinanie i ochrona krawędzi.
- `Runtime/FlowVolume.cs`, `WaterMotion.cs`, `WaterDrip.cs` — nurty, refleksy i krople.
- `Runtime/SewerLadder.cs`, `RatPatrol.cs` — drabiny i patrole.
- `Runtime/SewerCamera.cs`, `SewerLighting.cs`, `SewerCutaway.cs`, `RunnerHud.cs` — kamera, światło i interfejs.
- `Runtime/SewerWorld.cs` — przepis do tworzenia edytowalnej sceny.
- `Editor/` — otwieranie, odtwarzanie, walidacja oraz uruchamianie testów.
- `Tests/PlayMode/PrototypeTests.cs` — sprawdzanie aktualnych zasad i pełnej drogi przez oba poziomy.

## Sprawdzenie

**Tools → Poopline → Run PlayMode checks** uruchamia testy w otwartym Editorze. Wynik trafia do `TestResults/PlayMode-0.4.xml`, postęp do `TestResults/0.4-progress.txt`, a zmierzone czasy pełnej trasy do `TestResults/stage-times.txt`. Dodatkowe **Run tunnel visibility checks** sprawdza kamerę, światło i odsłanianie postaci; zapisuje `TestResults/Visibility-0.4.xml`. Wyniki i zakres ręcznego sprawdzenia są w `VALIDATION.md`.

Silnik i pakiety pozostały bez zmian. To wersja do testowania w Unity Editor na Macu, działająca offline. Nie przygotowano nowego wydania macOS ani wersji mobilnych; stary plik w `Builds` nie reprezentuje 0.4. Grafika i animacje pozostają prototypowe. Nie dodano nagrań dźwiękowych. Zmiany nie są commitowane ani stage’owane.
