﻿using System.Collections.Generic;

public class ControlMatrix
{
    public static CastingCommand GetCastingCommand(CastingStatus previousStatus, ActionControlState previousControl, ActionControlState currentControl)
    {
        Layer layer = _controlMatrix[previousStatus];
        Row row = layer[previousControl];

        return row[currentControl];
    }

    private static Dictionary<CastingStatus, Layer> _controlMatrix = new Dictionary<CastingStatus, Layer>
    {
        {
            CastingStatus.Cooldown,
            new Layer(
                new Row(
                    CastingCommand.PrecastLeft,
                    CastingCommand.None,
                    CastingCommand.None,
                    CastingCommand.None
                ),
                new Row(
                    CastingCommand.None,
                    CastingCommand.PrecastRight,
                    CastingCommand.None,
                    CastingCommand.None
                ),
                new Row(
                    CastingCommand.None
                ),
                new Row(
                    CastingCommand.PrecastLeft,
                    CastingCommand.PrecastRight,
                    CastingCommand.None,
                    CastingCommand.None
                )
            )
        },
        {
            CastingStatus.Ready,
            new Layer(
                new Row(
                    CastingCommand.None,
                    CastingCommand.CastRight,
                    CastingCommand.CastRight,
                    CastingCommand.None
                ),
                new Row(
                    CastingCommand.CastLeft,
                    CastingCommand.None,
                    CastingCommand.CastLeft,
                    CastingCommand.None
                ),
                new Row(
                    CastingCommand.None
                ),
                new Row(
                    CastingCommand.CastLeft,
                    CastingCommand.CastRight,
                    CastingCommand.CastLeft,
                    CastingCommand.None
                )
            )
        }
    };

    internal class Layer : Dictionary<ActionControlState, Row>
    {
        internal Layer(
            Row leftLastFrame,
            Row rightLastFrame,
            Row bothLastFrame,
            Row noneLastFrame
        )
        {
            this[ActionControlState.Left] = leftLastFrame;
            this[ActionControlState.Right] = rightLastFrame;
            this[ActionControlState.Both] = bothLastFrame;
            this[ActionControlState.None] = noneLastFrame;
        }
    }

    internal class Row : Dictionary<ActionControlState, CastingCommand> {
        internal Row(
            CastingCommand leftThisFrame,
            CastingCommand rightThisFrame,
            CastingCommand bothThisFrame,
            CastingCommand noneThisFrame
        )
        {
            this[ActionControlState.Left] = leftThisFrame;
            this[ActionControlState.Right] = rightThisFrame;
            this[ActionControlState.Both] = bothThisFrame;
            this[ActionControlState.None] = noneThisFrame;
        }

        internal Row(CastingCommand fixedStatus) : this(fixedStatus, fixedStatus, fixedStatus, fixedStatus) { }
    }
}