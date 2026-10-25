# Sprawdzenie prototypu 0.2.0

Data: 23 października 2026. Unity: **6000.5.1f1**, macOS, lokalny projekt.

## Wykonane sprawdzenia

- Walidacja sceny i listy scen budowania zakończyła się `POOPLINE_PROJECT_VALIDATED`.
- Zbudowano aplikację macOS 0.2.0: `Build Finished, Result: Success`. Przed porządkowaniem projektu uruchomiono ją i sprawdzono wizualnie mapę Polski, rozmieszczenie sześciu nazw miast oraz przycisk wejścia do kanałów.
- **PlayMode: 9 testów, 9 zaliczonych, 0 nieudanych** w pełnym przebiegu (około 107 sekund).
- Sprawdzono, że postać nie przemieszcza się w poziomie bez kierunku, można iść w prawo, zatrzymać się po puszczeniu klawisza oraz zawrócić w lewo.
- Test trasy przeszedł wszystkie sześć miast na normalnej fizyce, sterując ruchem i skokiem, bez teleportowania do mety. W tym teście czas gry był przyspieszony dwukrotnie; krok fizyki pozostał bez zmian.
- Sprawdzono wybór miasta przez przycisk mapy, wejście do Gdańska, powrót do mapy, zatrzymanie rozgrywki i rozpoczęcie nowej próby w Krakowie.
- Sprawdzono jednoczesne naciśnięcie kierunku i skoku osobnymi identyfikatorami palców, zwalnianie kierunku po wyjściu z przycisku oraz brak zablokowanego ruchu po pauzie.
- Sprawdzono przegraną na przeszkodzie, punkty, ich przywrócenie po restarcie, zatrzymanie pozycji i czasu w pauzie, wznowienie oraz zerowanie próby.
- Sprawdzono wejście z klawiatury i pada oraz brak podwójnego skoku w powietrzu.
- Test ukończenia tras sprawdził zapis rekordu pod osobnym kluczem każdego miasta. Testy przywracają wcześniejsze rekordy i wybrane miasto.

W testach bez okna zdarzenia Input System są tymczasowo kierowane do gry niezależnie od fokusu. Po testach przywracane są wcześniejsze ustawienia oraz skala czasu.

Wynik PlayMode i log budowania są w lokalnym folderze `TestResults`, pomijanym przez Git. Aplikacja znajduje się w `Builds/macOS/Poopline.app`.

## Granice tego sprawdzenia

- Nie testowano fizycznego iPhone'a, iPada, urządzenia z Androidem ani fizycznego kontrolera.
- Test przycisków dotykowych nie potwierdza obsługi wszystkich urządzeń, notchów ani gestów systemowych. Safe area i skalowanie są zaimplementowane, lecz wymagają oceny na urządzeniu.
- Nie budowano iOS, Androida, Windows ani Linuksa. Lokalna instalacja Unity ma moduły macOS i WebGL; pozostałe trzeba doinstalować.
- Nie wykonywano podpisywania dystrybucyjnego, publikacji App Store / Google Play / Steam ani wysyłki do zewnętrznych usług.
- Nie sprawdzano importu projektu na drugim komputerze. Kompilacja, PlayMode i build dotyczą lokalnego katalogu projektu; sprawdzenia po porządkowaniu opisano poniżej.

## Porządkowanie projektu

Na prośbę użytkownika usunięto stare sceny `SampleScene` i `GamePrototype`, wcześniejsze skrypty i generatory, zasoby Platformer Microgame, samouczki, nieużywane prefaby, grafiki, dźwięki oraz zasoby TextMesh Pro. W `Assets` pozostał wyłącznie folder `Poopline` z bieżącą grą, testami, narzędziami i przeniesionymi ustawieniami URP. GUID-y potrzebnych ustawień zachowano.

Liczba plików w `Assets`, łącznie z `.meta`, zmniejszyła się z 1037 do 49. Kontrola odwołań nie znalazła referencji do usuniętych zasobów. Sumy kontrolne potwierdziły, że kod gry, narzędzi i testów nie został zmieniony podczas porządkowania.

Po usunięciu plików i odświeżeniu zależności ponownie wykonano pełne testy PlayMode: **9/9 zaliczonych, 0 nieudanych**, 107,09 s. Obejmują wszystkie sześć miast oraz sterowanie, pauzę, restart i przegraną. Wersje pozostawionych pakietów nie zostały zmienione; usunięto 15 zbędnych zależności bezpośrednich i pośrednich.

Następnie walidacja sceny i budowanie aplikacji macOS również zakończyły się sukcesem. Bieżąca aplikacja w `Builds/macOS/Poopline.app` pochodzi z oczyszczonego projektu. Aktualne logi i wynik XML zapisano w `TestResults`.

Usunięto nieużywany zestaw akcji wejścia szablonu wraz z jego wpisami w ustawieniach. Usunięto też zależności Cinemachine, Unity Play, samouczków i Multiplayer Center. Zestaw pakietów 2D zastąpiono pojedynczym pakietem Sprite; renderowanie URP, Input System, UI, testy i narzędzia programistyczne pozostają.

Przed usuwaniem zapisano i sprawdzono kopię plików, w tym niezacommitowanych starych prototypów, w `/private/tmp/poopline-cleanup-20261023/before-cleanup.tar.gz`. Lista usuniętych plików i sumy kontrolne są obok archiwum. Jest to kopia tymczasowa poza projektem, nie zamiennik repozytorium ani trwałego backupu.

Nie utworzono commitów, nie zmieniano historii Git i nie wykonywano push.
