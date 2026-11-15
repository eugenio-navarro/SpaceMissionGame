using System;
using System.Collections.Generic;
using UnityEngine;

// Muestra en 3D las piezas elegidas: cada pieza tiene su punto fijo en el satélite
// y el satélite se apoya arriba del cohete elegido.
public class SatelliteVisual : MonoBehaviour
{
    [Serializable]
    public class Entry
    {
        public string id;
        public GameObject root;
        public Transform top; // solo cohetes: punto donde se apoya el satélite
    }

    public HangarController controller;
    public Transform satellite;
    public Transform noRocketTop;
    public List<Entry> parts = new List<Entry>();
    public List<Entry> rockets = new List<Entry>();

    void Start()
    {
        if (controller == null) return; // en Resultados se llama a Show() directamente
        controller.Changed += Refresh;
        Refresh();
    }

    void OnDestroy()
    {
        if (controller != null) controller.Changed -= Refresh;
    }

    void Refresh() => Show(controller.Design);

    public void Show(MissionDesign d)
    {
        foreach (var e in parts) e.root.SetActive(IsOn(d, e.id));
        if (noRocketTop == null) return; // sin cohetes (Resultados)

        Transform top = noRocketTop;
        foreach (var e in rockets)
        {
            bool on = d.rocket != null && d.rocket.id == e.id;
            e.root.SetActive(on);
            if (on) top = e.top;
        }
        satellite.position = top.position;
    }

    static bool IsOn(MissionDesign d, string id) =>
        d.instruments.Exists(p => p.id == id)
        || (d.panel != null && d.panel.id == id)
        || (d.battery != null && d.battery.id == id)
        || (d.antenna != null && d.antenna.id == id);
}
