// Sovereign Engine
// Copyright (c) 2024 opticfluorine
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using Sovereign.EcsBenchmark;

var size = 65536;

Console.WriteLine("=================================");
Console.WriteLine("ECS Benchmark, Kinematics Updates");
Console.WriteLine($"Size: {size}");
Console.WriteLine("=================================");
Console.WriteLine();

var benchmark = new EcsBenchmarkInstance(size);
var results = benchmark.Run();
Console.WriteLine($"Measurement Count:            {results.RunCount}");
Console.WriteLine($"Mean Runtime (us):            {results.MeanRuntimeUs}");
Console.WriteLine($"Mean Runtime Per Entity (us): {results.MeanRuntimeUs / size}");
Console.WriteLine($"Minimum Runtime (us):         {results.MinRuntimeUs}");
Console.WriteLine($"Maximum Runtime (us):         {results.MaxRuntimeUs}");
Console.WriteLine();