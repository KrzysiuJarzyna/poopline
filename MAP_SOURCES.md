# Mapa wyboru miasta

Obrys Polski pochodzi z Natural Earth, warstwy admin-0 w skali 1:110m. Wybrano wyłącznie poligon `ADM0_A3=POL`, usunięto powtórzony punkt zamykający i zapisano współrzędne w `PolandMapGraphic.cs`. UI tworzy z nich siatkę trójkątów; gra nie pobiera map podczas działania.

- [Dane Natural Earth](https://www.naturalearthdata.com/downloads/110m-cultural-vectors/110m-admin-0-countries/)
- [Źródłowy GeoJSON](https://github.com/nvkelso/natural-earth-vector/blob/master/geojson/ne_110m_admin_0_countries.geojson)
- [Warunki wykorzystania](https://www.naturalearthdata.com/about/terms-of-use/) — dane są w domenie publicznej.

Pobrano 23 października 2026. Punkty sześciu miast są przybliżonymi położeniami ich centrów, zapisanymi w `CityCatalog.cs`. Mapa służy do wyboru poziomu. Kanały są fikcyjne; współrzędne i obrys kraju nie służą do wyznaczania rzeczywistych sieci podziemnych.
