namespace Landoria.DecayControl
{
    internal sealed class DecayControlServerConfiguration
    {
        internal DecayControlMode FuelConsumption { get; private set; }
        internal DecayControlMode EnvironmentalBuildingWear { get; private set; }

        internal static DecayControlServerConfiguration FromArguments(string[] arguments)
        {
            return new DecayControlServerConfiguration
            {
                FuelConsumption = DecayControlArgumentPolicy.Resolve(arguments,
                    "--decay-control-fuel-consumption", DecayControlMode.Default),
                EnvironmentalBuildingWear = DecayControlArgumentPolicy.Resolve(arguments,
                    "--decay-control-environmental-building-wear", DecayControlMode.Default)
            };
        }
    }
}
