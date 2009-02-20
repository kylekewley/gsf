//*******************************************************************************************************
//  IConfigurationCellParsingState.cs
//  Copyright © 2009 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR BK-C
//       Phone: 423/751-4165
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  04/16/2005 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

namespace PCS.PhasorProtocols
{
    /// <summary>
    /// Defines function signature for creating new <see cref="IChannelDefinition"/> objects.
    /// </summary>
    /// <param name="parent">Reference to parent <see cref="IConfigurationCell"/>.</param>
    /// <param name="binaryImage">Binary image to parse <see cref="IChannelDefinition"/> from.</param>
    /// <param name="startIndex">Start index into <paramref name="binaryImage"/> to begin parsing.</param>
    /// <returns>New <see cref="IChannelDefinition"/> object.</returns>
    /// <typeparam name="T">Specific <see cref="IChannelDefinition"/> type of object that gets created by referenced function.</typeparam>
    public delegate T CreateNewDefinitionFunctionSignature<T>(IConfigurationCell parent, byte[] binaryImage, int startIndex) where T : IChannelDefinition;

    /// <summary>
    /// Represents a protocol independent interface representation of the parsing state of any kind of <see cref="IConfigurationCell"/>.
    /// </summary>
    public interface IConfigurationCellParsingState : IChannelCellParsingState
    {
        /// <summary>
        /// Gets reference to <see cref="CreateNewDefinitionFunctionSignature{T}"/> delegate used to create new <see cref="IPhasorDefinition"/> objects.
        /// </summary>
        CreateNewDefinitionFunctionSignature<IPhasorDefinition> CreateNewPhasorDefinitionFunction { get; }

        /// <summary>
        /// Gets reference to <see cref="CreateNewDefinitionFunctionSignature{T}"/> delegate used to create new <see cref="IFrequencyDefinition"/> objects.
        /// </summary>
        CreateNewDefinitionFunctionSignature<IFrequencyDefinition> CreateNewFrequencyDefinitionFunction { get; }

        /// <summary>
        /// Gets reference to <see cref="CreateNewDefinitionFunctionSignature{T}"/> delegate used to create new <see cref="IAnalogDefinition"/> objects.
        /// </summary>
        CreateNewDefinitionFunctionSignature<IAnalogDefinition> CreateNewAnalogDefinitionFunction { get; }

        /// <summary>
        /// Gets reference to <see cref="CreateNewDefinitionFunctionSignature{T}"/> delegate used to create new <see cref="IDigitalDefinition"/> objects.
        /// </summary>
        CreateNewDefinitionFunctionSignature<IDigitalDefinition> CreateNewDigitalDefinitionFunction { get; }
    }
}