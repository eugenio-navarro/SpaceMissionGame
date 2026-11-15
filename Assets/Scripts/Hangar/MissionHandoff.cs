using System.Linq;

// Puente entre el Hangar y las escenas Lanzamiento/Resultados: arma la "foto" de la misión
// (ResultsInfo) a partir del diseño y su cálculo. TEMPORAL hasta que exista MissionState.
public static class MissionHandoff
{
    public static ResultsInfo Build(MissionDesign design, MissionReport report, ContractSpec contract)
    {
        var hungriest = design.instruments.OrderByDescending(i => i.power).FirstOrDefault();
        return new ResultsInfo
        {
            contractTitle = contract.title,
            objective = contract.objective,
            destination = contract.destination,
            similarMission = RealMissions.For(contract, design).name, // radar en la Tierra → SAOCOM (CONAE)
            scienceGoal = contract.scienceGoal,
            budget = contract.budget,

            rocketId = design.rocket.id,
            rocketName = design.rocket.displayName,
            orbitId = design.orbit.id,
            orbitName = design.orbit.displayName,
            stations = design.stations,
            mass = report.mass,
            capacity = report.capacity,
            consumption = report.consumption,
            generation = report.generation,
            dataGenerated = report.dataGenerated,
            downlink = report.downlink,
            detail = contract.DetailFor(design.orbit.id),
            cost = report.cost,
            riskPercent = report.riskPercent,
            hungriestInstrument = hungriest != null ? hungriest.displayName : "",
            hungriestInstrumentData = hungriest != null ? hungriest.dataRate : 0f,

            design = TradeStudy.Clone(design),
        };
    }
}
