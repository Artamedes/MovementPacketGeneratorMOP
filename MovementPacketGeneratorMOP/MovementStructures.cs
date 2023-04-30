using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Xml.Linq;
using System.Diagnostics;

namespace MovementPacketGeneratorMOP
{
    public class MovementStructures
    {
        public static MovementStructures Instance = new();

        void WriteServerMessageData(ref StringBuilder sb, ref MovementStatusElements[] sequence)
        {
            bool isInTransport = false;
            bool isInFall = false;
            bool isInFallVelocity = false;

            uint tabs = 1;

            foreach (var element in sequence)
            {
                bool isTransportElement = false;

                switch (element)
                {
                    case MovementStatusElements.MSEHasTransportGuidByte0:
                    case MovementStatusElements.MSEHasTransportGuidByte1:
                    case MovementStatusElements.MSEHasTransportGuidByte2:
                    case MovementStatusElements.MSEHasTransportGuidByte3:
                    case MovementStatusElements.MSEHasTransportGuidByte4:
                    case MovementStatusElements.MSEHasTransportGuidByte5:
                    case MovementStatusElements.MSEHasTransportGuidByte6:
                    case MovementStatusElements.MSEHasTransportGuidByte7:
                    case MovementStatusElements.MSEHasTransportTime2:
                    case MovementStatusElements.MSETransportGuidByte0:
                    case MovementStatusElements.MSETransportGuidByte1:
                    case MovementStatusElements.MSETransportGuidByte2:
                    case MovementStatusElements.MSETransportGuidByte3:
                    case MovementStatusElements.MSETransportGuidByte4:
                    case MovementStatusElements.MSETransportGuidByte5:
                    case MovementStatusElements.MSETransportGuidByte6:
                    case MovementStatusElements.MSETransportGuidByte7:
                    case MovementStatusElements.MSETransportPositionX:
                    case MovementStatusElements.MSETransportPositionY:
                    case MovementStatusElements.MSETransportPositionZ:
                    case MovementStatusElements.MSETransportOrientation:
                    case MovementStatusElements.MSETransportSeat:
                    case MovementStatusElements.MSETransportTime:
                    case MovementStatusElements.MSETransportTime2:
                    case MovementStatusElements.MSETransportVehicleId:
                    case MovementStatusElements.MSEHasVehicleId:
                        isTransportElement = true;
                        break;
                    default:
                        if (isInTransport)
                        {
                            isInTransport = false;
                            tabs--;

                            sb.AppendLine();
                            for (int i = 0; i < tabs; ++i)
                                sb.Append("    ");
                            sb.Append("}");
                        }
                        break;
                }

                switch (element)
                {
                    case MovementStatusElements.MSEFallCosAngle:
                    case MovementStatusElements.MSEFallSinAngle:
                    case MovementStatusElements.MSEFallHorizontalSpeed:
                        if (!isInFallVelocity)
                        {
                            isInFallVelocity = true;

                            sb.AppendLine();
                            for (int i = 0; i < tabs; ++i)
                                sb.Append("    ");

                            if (!isInFall)
                                sb.Append("if (Status.Fall && Status.Fall->Velocity)");
                            else
                                sb.Append("if (Status.Fall->Velocity)");

                            sb.AppendLine();
                            for (int i = 0; i < tabs; ++i)
                                sb.Append("    ");
                            sb.Append("{");
                            tabs++;
                        }
                        break;
                    default:
                        if (isInFallVelocity)
                        {
                            isInFallVelocity = false;
                            tabs--;
                            sb.AppendLine();
                            for (int i = 0; i < tabs; ++i)
                                sb.Append("    ");
                            sb.Append("}");
                        }
                        break;
                }

                switch (element)
                {
                    case MovementStatusElements.MSEFallTime:
                    case MovementStatusElements.MSEFallVerticalSpeed:
                    case MovementStatusElements.MSEHasFallDirection:
                        if (!isInFall)
                        {
                            isInFall = true;
                            sb.AppendLine();
                            for (int i = 0; i < tabs; ++i)
                                sb.Append("    ");
                            sb.Append("if (Status.Fall)");
                            sb.AppendLine();
                            for (int i = 0; i < tabs; ++i)
                                sb.Append("    ");
                            sb.Append("{");
                            tabs++;
                        }
                        break;

                    case MovementStatusElements.MSEFallCosAngle:
                    case MovementStatusElements.MSEFallSinAngle:
                    case MovementStatusElements.MSEFallHorizontalSpeed:
                        break;
                    default:
                        if (isInFall)
                        {
                            isInFall = false;
                            tabs--;
                            sb.AppendLine();
                            for (int i = 0; i < tabs; ++i)
                                sb.Append("    ");
                            sb.Append("}");
                        }
                        break;
                }

                if (isTransportElement && !isInTransport)
                {
                    isInTransport = true;
                    sb.AppendLine();
                    for (int i = 0; i < tabs; ++i)
                        sb.Append("    ");
                    sb.Append("if (Status.Transport)");
                    sb.AppendLine();
                    for (int i = 0; i < tabs; ++i)
                        sb.Append("    ");
                    sb.Append("{");

                    tabs++;
                }

                if (element == MovementStatusElements.MSEEnd)
                    break;

                sb.AppendLine();
                for (int i = 0; i < tabs; ++i)
                    sb.Append("    ");

                if (element >= MovementStatusElements.MSEHasGuidByte0 && element <= MovementStatusElements.MSEHasGuidByte7)
                {
                    bool needsStatus = sequence.Length > 25;
                    sb.Append($"_worldPacket.WriteBit({(needsStatus ? "Status." : "")}MoverGUID[{element - MovementStatusElements.MSEHasGuidByte0}]);");
                    continue;
                }

                if (element >= MovementStatusElements.MSEHasTransportGuidByte0 &&
                    element <= MovementStatusElements.MSEHasTransportGuidByte7)
                {   
                    sb.Append($"_worldPacket.WriteBit(Status.Transport->Guid[{element - MovementStatusElements.MSEHasTransportGuidByte0}]);");
                    continue;
                }

                if (element >= MovementStatusElements.MSEGuidByte0 && element <= MovementStatusElements.MSEGuidByte7)
                {
                    bool needsStatus = sequence.Length > 25;
                    sb.Append($"_worldPacket.WriteByteSeq({(needsStatus ? "Status." : "")}MoverGUID[{element - MovementStatusElements.MSEGuidByte0}]);");
                    continue;
                }

                if (element >= MovementStatusElements.MSETransportGuidByte0 &&
                    element <= MovementStatusElements.MSETransportGuidByte7)
                {
                    sb.Append($"_worldPacket.WriteByteSeq(Status.Transport->Guid[{element - MovementStatusElements.MSETransportGuidByte0}]);");
                    continue;
                }

                switch (element)
                {
                    case MovementStatusElements.MSEHasRemoveForcesIDs:
                        sb.Append("_worldPacket.WriteBits(Status.RemoveForcesIDs.size(), 22);");
                        break;
                    case MovementStatusElements.MSERemoveForcesIDs:
                        sb.Append("for (uint32 force : Status.RemoveForcesIDs)");
                        sb.AppendLine();
                        for (int i = 0; i < tabs; ++i)
                            sb.Append("    ");
                        sb.Append("    _worldPacket << force;");
                        break;
                    case MovementStatusElements.MSEHasMovementFlags:
                        sb.Append("_worldPacket.WriteBit(!Status.MoveFlags0.has_value());");
                        break;
                    case MovementStatusElements.MSEHasMovementFlags2:
                        sb.Append("_worldPacket.WriteBit(!Status.MoveFlags1.has_value());");
                        break;
                    case MovementStatusElements.MSEHasMoveTime:
                        sb.Append("_worldPacket.WriteBit(!Status.MoveTime.has_value());");
                        break;
                    case MovementStatusElements.MSEHasOrientation:
                        sb.Append("_worldPacket.WriteBit(!Status.Facing.has_value());");
                        break;
                    case MovementStatusElements.MSEHasPitch:
                        sb.Append("_worldPacket.WriteBit(!Status.Pitch.has_value());");
                        break;
                    case MovementStatusElements.MSEHasSplineElevation:
                        sb.Append("_worldPacket.WriteBit(!Status.StepUpStartElevation.has_value());");
                        break;
                    case MovementStatusElements.MSEHasTransportData:
                        sb.Append("_worldPacket.WriteBit(Status.Transport.has_value());");
                        break;
                    case MovementStatusElements.MSEHasTransportTime2:
                        sb.Append("_worldPacket.WriteBit(Status.Transport->PrevMoveTime.has_value());");
                        break;
                    case MovementStatusElements.MSEHasVehicleId:
                        sb.Append("_worldPacket.WriteBit(Status.Transport->VehicleRecID.has_value());");
                        break;
                    case MovementStatusElements.MSEHasFallData:
                        sb.Append("_worldPacket.WriteBit(Status.Fall.has_value());");
                        break;
                    case MovementStatusElements.MSEHasFallDirection:
                        sb.Append("_worldPacket.WriteBit(Status.Fall->Velocity.has_value());");
                        break;
                    case MovementStatusElements.MSEHasSpline:
                        sb.Append("_worldPacket.WriteBit(Status.HasSpline);");
                        break;
                    case MovementStatusElements.MSEMovementFlags:
                        sb.Append("if (Status.MoveFlags0)");
                        sb.AppendLine();
                        for (int i = 0; i < tabs; ++i)
                            sb.Append("    ");
                        sb.Append("    _worldPacket.WriteBits(*Status.MoveFlags0, 30);");
                        break;
                    case MovementStatusElements.MSEMovementFlags2:
                        sb.Append("if (Status.MoveFlags1)");
                        sb.AppendLine();
                        for (int i = 0; i < tabs; ++i)
                            sb.Append("    ");
                        sb.Append("    _worldPacket.WriteBits(*Status.MoveFlags1, 13);");
                        break;
                    case MovementStatusElements.MSEMoveTime:
                        sb.Append("if (Status.MoveTime)");
                        sb.AppendLine();
                        for (int i = 0; i < tabs; ++i)
                            sb.Append("    ");
                        sb.Append("    _worldPacket << *Status.MoveTime;");
                        break;
                    case MovementStatusElements.MSEPositionX:
                        sb.Append("_worldPacket << Status.Pos.m_positionX;");
                        break;
                    case MovementStatusElements.MSEPositionY:
                        sb.Append("_worldPacket << Status.Pos.m_positionY;");
                        break;
                    case MovementStatusElements.MSEPositionZ:
                        sb.Append("_worldPacket << Status.Pos.m_positionZ;");
                        break;
                    case MovementStatusElements.MSEOrientation:
                        sb.Append("if (Status.Facing)");
                        sb.AppendLine();
                        for (int i = 0; i < tabs; ++i)
                            sb.Append("    ");
                        sb.Append("    _worldPacket << *Status.Facing;");
                        break;
                    case MovementStatusElements.MSETransportPositionX:
                        sb.Append("_worldPacket << Status.Transport->Pos.m_positionX;");
                        break;
                    case MovementStatusElements.MSETransportPositionY:
                        sb.Append("_worldPacket << Status.Transport->Pos.m_positionY;");
                        break;
                    case MovementStatusElements.MSETransportPositionZ:
                        sb.Append("_worldPacket << Status.Transport->Pos.m_positionZ;");
                        break;
                    case MovementStatusElements.MSETransportOrientation:
                        sb.Append("_worldPacket << Position::NormalizeOrientation(Status.Transport->Facing);");
                        break;
                    case MovementStatusElements.MSETransportSeat:
                        sb.Append("_worldPacket << Status.Transport->VehicleSeatIndex;");
                        break;
                    case MovementStatusElements.MSETransportTime:
                        sb.Append("_worldPacket << Status.Transport->MoveTime;");
                        break;
                    case MovementStatusElements.MSETransportTime2:
                        sb.Append("if (Status.Transport->PrevMoveTime)");
                        sb.AppendLine();
                        for (int i = 0; i < tabs; ++i)
                            sb.Append("    ");
                        sb.Append("    _worldPacket << *Status.Transport->PrevMoveTime;");
                        break;
                    case MovementStatusElements.MSETransportVehicleId:
                        sb.Append("if (Status.Transport->VehicleRecID)");
                        sb.AppendLine();
                        for (int i = 0; i < tabs; ++i)
                            sb.Append("    ");
                        sb.Append("    _worldPacket << *Status.Transport->VehicleRecID;");
                        break;
                    case MovementStatusElements.MSEPitch:
                        sb.Append("if (Status.Pitch)");
                        sb.AppendLine();
                        for (int i = 0; i < tabs; ++i)
                            sb.Append("    ");
                        sb.Append("    _worldPacket << Position::NormalizePitch(*Status.Pitch);");
                        break;
                    case MovementStatusElements.MSEFallTime:
                        sb.Append("_worldPacket << Status.Fall->Time;");
                        break;
                    case MovementStatusElements.MSEFallVerticalSpeed:
                        sb.Append("_worldPacket << Status.Fall->JumpVelocity;");
                        break;
                    case MovementStatusElements.MSEFallCosAngle:
                        sb.Append("_worldPacket << Status.Fall->Velocity->Direction.x;");
                        break;
                    case MovementStatusElements.MSEFallSinAngle:
                        sb.Append("_worldPacket << Status.Fall->Velocity->Direction.y;");
                        break;
                    case MovementStatusElements.MSEFallHorizontalSpeed:
                        sb.Append("_worldPacket << Status.Fall->Velocity->Speed;");
                        break;
                    case MovementStatusElements.MSESplineElevation:
                        sb.Append("if (Status.StepUpStartElevation)");
                        sb.AppendLine();
                        for (int i = 0; i < tabs; ++i)
                            sb.Append("    ");
                        sb.Append("    _worldPacket << *Status.StepUpStartElevation;");
                        break;
                    case MovementStatusElements.MSEHeightChangeFailed:
                        sb.Append("_worldPacket.WriteBit(Status.HeightChangeFailed);");
                        break;
                    case MovementStatusElements.MSERemoteTimeValid:
                        sb.Append("_worldPacket.WriteBit(Status.RemoteTimeValid);");
                        break;
                    case MovementStatusElements.MSEHasMoveIdx:
                        sb.Append("_worldPacket.WriteBit(!Status.MoveIndex.has_value());");
                        break;
                    case MovementStatusElements.MSEMoveIdx:
                        sb.Append("if (Status.MoveIndex)");
                        sb.AppendLine();
                        for (int i = 0; i < tabs; ++i)
                            sb.Append("    ");
                        sb.Append("    _worldPacket << *Status.MoveIndex;");
                        break;
                    case MovementStatusElements.MSESpeed:
                        sb.Append("_worldPacket << Speed;");
                        break;
                    case MovementStatusElements.MSESequenceIndex:
                        sb.Append("_worldPacket << SequenceIndex;");
                        break;
                    default:
                        Debug.Assert(false, "Incorrect sequence element detected at WriteServerMessageData");
                        break;
                }
            }
        }


        void WriteClientMessageData(ref StringBuilder sb, ref MovementStatusElements[] sequence)
        {
            bool isInTransport = false;
            bool isInFall = false;
            bool isInFallVelocity = false;

            uint tabs = 1;

            foreach (var element in sequence)
            {
                bool isTransportElement = false;

                switch (element)
                {
                    case MovementStatusElements.MSEHasTransportGuidByte0:
                    case MovementStatusElements.MSEHasTransportGuidByte1:
                    case MovementStatusElements.MSEHasTransportGuidByte2:
                    case MovementStatusElements.MSEHasTransportGuidByte3:
                    case MovementStatusElements.MSEHasTransportGuidByte4:
                    case MovementStatusElements.MSEHasTransportGuidByte5:
                    case MovementStatusElements.MSEHasTransportGuidByte6:
                    case MovementStatusElements.MSEHasTransportGuidByte7:
                    case MovementStatusElements.MSEHasTransportTime2:
                    case MovementStatusElements.MSETransportGuidByte0:
                    case MovementStatusElements.MSETransportGuidByte1:
                    case MovementStatusElements.MSETransportGuidByte2:
                    case MovementStatusElements.MSETransportGuidByte3:
                    case MovementStatusElements.MSETransportGuidByte4:
                    case MovementStatusElements.MSETransportGuidByte5:
                    case MovementStatusElements.MSETransportGuidByte6:
                    case MovementStatusElements.MSETransportGuidByte7:
                    case MovementStatusElements.MSETransportPositionX:
                    case MovementStatusElements.MSETransportPositionY:
                    case MovementStatusElements.MSETransportPositionZ:
                    case MovementStatusElements.MSETransportOrientation:
                    case MovementStatusElements.MSETransportSeat:
                    case MovementStatusElements.MSETransportTime:
                    case MovementStatusElements.MSETransportTime2:
                    case MovementStatusElements.MSETransportVehicleId:
                    case MovementStatusElements.MSEHasVehicleId:
                        isTransportElement = true;
                        break;
                    default:
                        if (isInTransport)
                        {
                            isInTransport = false;
                            tabs--;

                            sb.AppendLine();
                            for (int i = 0; i < tabs; ++i)
                                sb.Append("    ");
                            sb.Append("}");
                        }
                        break;
                }

                switch (element)
                {
                    case MovementStatusElements.MSEFallCosAngle:
                    case MovementStatusElements.MSEFallSinAngle:
                    case MovementStatusElements.MSEFallHorizontalSpeed:
                        if (!isInFallVelocity)
                        {
                            isInFallVelocity = true;

                            sb.AppendLine();
                            for (int i = 0; i < tabs; ++i)
                                sb.Append("    ");

                            if (!isInFall)
                                sb.Append("if (Status.Fall && Status.Fall->Velocity)");
                            else
                                sb.Append("if (Status.Fall->Velocity)");

                            sb.AppendLine();
                            for (int i = 0; i < tabs; ++i)
                                sb.Append("    ");
                            sb.Append("{");
                            tabs++;
                        }
                        break;
                    default:
                        if (isInFallVelocity)
                        {
                            isInFallVelocity = false;
                            tabs--;
                            sb.AppendLine();
                            for (int i = 0; i < tabs; ++i)
                                sb.Append("    ");
                            sb.Append("}");
                        }
                        break;
                }

                switch (element)
                {
                    case MovementStatusElements.MSEFallTime:
                    case MovementStatusElements.MSEFallVerticalSpeed:
                    case MovementStatusElements.MSEHasFallDirection:
                        if (!isInFall)
                        {
                            isInFall = true;
                            sb.AppendLine();
                            for (int i = 0; i < tabs; ++i)
                                sb.Append("    ");
                            sb.Append("if (Status.Fall)");
                            sb.AppendLine();
                            for (int i = 0; i < tabs; ++i)
                                sb.Append("    ");
                            sb.Append("{");
                            tabs++;
                        }
                        break;

                    case MovementStatusElements.MSEFallCosAngle:
                    case MovementStatusElements.MSEFallSinAngle:
                    case MovementStatusElements.MSEFallHorizontalSpeed:
                        break;
                    default:
                        if (isInFall)
                        {
                            isInFall = false;
                            tabs--;
                            sb.AppendLine();
                            for (int i = 0; i < tabs; ++i)
                                sb.Append("    ");
                            sb.Append("}");
                        }
                        break;
                }

                if (isTransportElement && !isInTransport)
                {
                    isInTransport = true;
                    sb.AppendLine();
                    for (int i = 0; i < tabs; ++i)
                        sb.Append("    ");
                    sb.Append("if (Status.Transport)");
                    sb.AppendLine();
                    for (int i = 0; i < tabs; ++i)
                        sb.Append("    ");
                    sb.Append("{");

                    tabs++;
                }

                if (element == MovementStatusElements.MSEEnd)
                    break;

                sb.AppendLine();
                for (int i = 0; i < tabs; ++i)
                    sb.Append("    ");

                if (element >= MovementStatusElements.MSEHasGuidByte0 && element <= MovementStatusElements.MSEHasGuidByte7)
                {
                    sb.Append($"Status.MoverGUID[{element - MovementStatusElements.MSEHasGuidByte0}] = _worldPacket.ReadBit();");
                    continue;
                }

                if (element >= MovementStatusElements.MSEHasTransportGuidByte0 &&
                    element <= MovementStatusElements.MSEHasTransportGuidByte7)
                {
                    sb.Append($"Status.Transport->Guid[{element - MovementStatusElements.MSEHasTransportGuidByte0}] = _worldPacket.ReadBit();");
                    continue;
                }

                if (element >= MovementStatusElements.MSEGuidByte0 && element <= MovementStatusElements.MSEGuidByte7)
                {
                    sb.Append($" _worldPacket.ReadByteSeq(Status.MoverGUID[{element - MovementStatusElements.MSEGuidByte0}]);");
                    continue;
                }

                if (element >= MovementStatusElements.MSETransportGuidByte0 &&
                    element <= MovementStatusElements.MSETransportGuidByte7)
                {
                    sb.Append($"_worldPacket.ReadByteSeq(Status.Transport->Guid[{element - MovementStatusElements.MSETransportGuidByte0}]);");
                    continue;
                }

                switch (element)
                {
                    case MovementStatusElements.MSEHasRemoveForcesIDs:
                        sb.Append("Status.RemoveForcesIDs.resize(_worldPacket.ReadBits(22));");
                        break;
                    case MovementStatusElements.MSERemoveForcesIDs:
                        sb.Append("for (uint32& force : Status.RemoveForcesIDs)");
                        sb.AppendLine();
                        for (int i = 0; i < tabs; ++i)
                            sb.Append("    ");
                        sb.Append("    _worldPacket >> force;");
                        break;
                    case MovementStatusElements.MSEHasMovementFlags:
                        sb.Append("if (!_worldPacket.ReadBit()) Status.MoveFlags0.emplace();");
                        break;
                    case MovementStatusElements.MSEHasMovementFlags2:
                        sb.Append("if (!_worldPacket.ReadBit()) Status.MoveFlags1.emplace();");
                        break;
                    case MovementStatusElements.MSEHasMoveTime:
                        sb.Append("if (!_worldPacket.ReadBit()) Status.MoveTime.emplace();");
                        break;
                    case MovementStatusElements.MSEHasOrientation:
                        sb.Append("if (!_worldPacket.ReadBit()) Status.Facing.emplace();");
                        break;
                    case MovementStatusElements.MSEHasPitch:
                        sb.Append("if (!_worldPacket.ReadBit()) Status.Pitch.emplace();");
                        break;
                    case MovementStatusElements.MSEHasMoveIdx:
                        sb.Append("if (!_worldPacket.ReadBit()) Status.MoveIndex.emplace();");
                        break;
                    case MovementStatusElements.MSEHasSplineElevation:
                        sb.Append("if (!_worldPacket.ReadBit()) Status.StepUpStartElevation.emplace();");
                        break;
                    case MovementStatusElements.MSEHasTransportData:
                        sb.Append("if (_worldPacket.ReadBit()) Status.Transport.emplace();");
                        break;
                    case MovementStatusElements.MSEHasTransportTime2:
                        sb.Append("if (_worldPacket.ReadBit()) Status.Transport->PrevMoveTime.emplace();");
                        break;
                    case MovementStatusElements.MSEHasVehicleId:
                        sb.Append("if (_worldPacket.ReadBit()) Status.Transport->VehicleRecID.emplace();");
                        break;
                    case MovementStatusElements.MSEHasFallData:
                        sb.Append("if (_worldPacket.ReadBit()) Status.Fall.emplace();");
                        break;
                    case MovementStatusElements.MSEHasFallDirection:
                        sb.Append("if (_worldPacket.ReadBit()) Status.Fall->Velocity.emplace();");
                        break;
                    case MovementStatusElements.MSEHasSpline:
                        sb.Append("Status.HasSpline = _worldPacket.ReadBit();");
                        break;
                    case MovementStatusElements.MSEMovementFlags:
                        sb.Append("if (Status.MoveFlags0)");
                        sb.AppendLine();
                        for (int i = 0; i < tabs; ++i)
                            sb.Append("    ");
                        sb.Append("    Status.MoveFlags0 = _worldPacket.ReadBits(30);");
                        break;
                    case MovementStatusElements.MSEMovementFlags2:
                        sb.Append("if (Status.MoveFlags1)");
                        sb.AppendLine();
                        for (int i = 0; i < tabs; ++i)
                            sb.Append("    ");
                        sb.Append("    Status.MoveFlags1 = _worldPacket.ReadBits(13);");
                        break;
                    case MovementStatusElements.MSEMoveTime:
                        sb.Append("if (Status.MoveTime)");
                        sb.AppendLine();
                        for (int i = 0; i < tabs; ++i)
                            sb.Append("    ");
                        sb.Append("    _worldPacket >> *Status.MoveTime;");
                        break;
                    case MovementStatusElements.MSEPositionX:
                        sb.Append("_worldPacket >> Status.Pos.m_positionX;");
                        break;
                    case MovementStatusElements.MSEPositionY:
                        sb.Append("_worldPacket >> Status.Pos.m_positionY;");
                        break;
                    case MovementStatusElements.MSEPositionZ:
                        sb.Append("_worldPacket >> Status.Pos.m_positionZ;");
                        break;
                    case MovementStatusElements.MSEOrientation:
                        sb.Append("if (Status.Facing)");
                        sb.AppendLine();
                        for (int i = 0; i < tabs; ++i)
                            sb.Append("    ");
                        sb.Append("    _worldPacket >> *Status.Facing;");
                        break;
                    case MovementStatusElements.MSETransportPositionX:
                        sb.Append("_worldPacket >> Status.Transport->Pos.m_positionX;");
                        break;
                    case MovementStatusElements.MSETransportPositionY:
                        sb.Append("_worldPacket >> Status.Transport->Pos.m_positionY;");
                        break;
                    case MovementStatusElements.MSETransportPositionZ:
                        sb.Append("_worldPacket >> Status.Transport->Pos.m_positionZ;");
                        break;
                    case MovementStatusElements.MSETransportOrientation:
                        sb.Append("_worldPacket >> Status.Transport->Facing;");
                        break;
                    case MovementStatusElements.MSETransportSeat:
                        sb.Append("_worldPacket >> Status.Transport->VehicleSeatIndex;");
                        break;
                    case MovementStatusElements.MSETransportTime:
                        sb.Append("_worldPacket >> Status.Transport->MoveTime;");
                        break;
                    case MovementStatusElements.MSETransportTime2:
                        sb.Append("if (Status.Transport->PrevMoveTime)");
                        sb.AppendLine();
                        for (int i = 0; i < tabs; ++i)
                            sb.Append("    ");
                        sb.Append("    _worldPacket >> *Status.Transport->PrevMoveTime;");
                        break;
                    case MovementStatusElements.MSETransportVehicleId:
                        sb.Append("if (Status.Transport->VehicleRecID)");
                        sb.AppendLine();
                        for (int i = 0; i < tabs; ++i)
                            sb.Append("    ");
                        sb.Append("    _worldPacket >> *Status.Transport->VehicleRecID;");
                        break;
                    case MovementStatusElements.MSEPitch:
                        sb.Append("if (Status.Pitch)");
                        sb.AppendLine();
                        for (int i = 0; i < tabs; ++i)
                            sb.Append("    ");
                        sb.Append("    _worldPacket >> *Status.Pitch;");
                        break;
                    case MovementStatusElements.MSEFallTime:
                        sb.Append("_worldPacket >> Status.Fall->Time;");
                        break;
                    case MovementStatusElements.MSEFallVerticalSpeed:
                        sb.Append("_worldPacket >> Status.Fall->JumpVelocity;");
                        break;
                    case MovementStatusElements.MSEFallCosAngle:
                        sb.Append("_worldPacket >> Status.Fall->Velocity->Direction.x;");
                        break;
                    case MovementStatusElements.MSEFallSinAngle:
                        sb.Append("_worldPacket >> Status.Fall->Velocity->Direction.y;");
                        break;
                    case MovementStatusElements.MSEFallHorizontalSpeed:
                        sb.Append("_worldPacket >> Status.Fall->Velocity->Speed;");
                        break;
                    case MovementStatusElements.MSESplineElevation:
                        sb.Append("if (Status.StepUpStartElevation)");
                        sb.AppendLine();
                        for (int i = 0; i < tabs; ++i)
                            sb.Append("    ");
                        sb.Append("    _worldPacket >> *Status.StepUpStartElevation;");
                        break;
                    case MovementStatusElements.MSEHeightChangeFailed:
                        sb.Append("Status.HeightChangeFailed = _worldPacket.ReadBit();");
                        break;
                    case MovementStatusElements.MSERemoteTimeValid:
                        sb.Append("Status.RemoteTimeValid = _worldPacket.ReadBit();");
                        break;
                    case MovementStatusElements.MSEMoveIdx:
                        sb.Append("if (Status.MoveIndex)");
                        sb.AppendLine();
                        for (int i = 0; i < tabs; ++i)
                            sb.Append("    ");
                        sb.Append("    _worldPacket >> *Status.MoveIndex;");
                        break;
                    case MovementStatusElements.MSESplineID:
                        sb.Append("_worldPacket >> SplineID;");
                        break;
                    case MovementStatusElements.MSESpeed:
                        sb.Append("_worldPacket >> Speed;");
                        break;
                    case MovementStatusElements.MSEAckIndex:
                        sb.Append("_worldPacket >> AckIndex;");
                        break;
                    case MovementStatusElements.MSEMountDisplayID:
                        sb.Append("_worldPacket >> MountDisplayID;");
                        break;
                    case MovementStatusElements.MSEHeight:
                        sb.Append("_worldPacket >> Height;");
                        break;
                    case MovementStatusElements.MSEReason:
                        sb.Append("Reason = _worldPacket.ReadBits(2);");
                        break;
                    default:
                        Debug.Assert(false, "Incorrect sequence element detected at WriteClientMessageData");
                        break;
                }
            }
        }

        public void DumpFunctions()
        {
            // Get all fields of this class
            var fields = typeof(MovementStructures547).GetFields();
            var fields2 = typeof(SpellMovementStructures547).GetFields();

            StringBuilder serverPacketStringBuilder = new();
            StringBuilder clientPacketStringBuilder = new();
            StringBuilder serverPacketHeaderStringBuilder = new();
            StringBuilder clientPacketHeaderStringBuilder = new();
            StringBuilder forwardDeclarations = new();

            serverPacketStringBuilder.AppendLine("/// This file is auto generated. Please don't edit!");
            serverPacketStringBuilder.AppendLine($"/// Date of auto generation: {DateTime.Now}");
            serverPacketStringBuilder.AppendLine();
            serverPacketStringBuilder.AppendLine("#include \"MovementPackets.h\"");
            serverPacketStringBuilder.AppendLine();

            clientPacketStringBuilder.AppendLine("/// This file is auto generated. Please don't edit!");
            clientPacketStringBuilder.AppendLine($"/// Date of auto generation: {DateTime.Now}");
            clientPacketStringBuilder.AppendLine();
            clientPacketStringBuilder.AppendLine("#include \"MovementPackets.h\"");
            clientPacketStringBuilder.AppendLine();

            serverPacketHeaderStringBuilder.AppendLine();
            clientPacketHeaderStringBuilder.AppendLine();

            foreach (var field in fields)
            {
                if (field.FieldType == typeof(MovementStatusElements[]))
                {
                    // Write the array to console
                    var value = field.GetValue(MovementStructures547.Instance);
                    if (value is MovementStatusElements[] elements)
                    {
                        string? opcodeName = null;

                        var serverPacketAttr = field.GetCustomAttribute<ServerPacketAttribute>();
                        var clientPacketAttr = field.GetCustomAttribute<ClientPacketAttribute>();

                        if (serverPacketAttr != null)
                            opcodeName = serverPacketAttr.Opcode;
                        else if (clientPacketAttr != null)
                            opcodeName = clientPacketAttr.Opcode;

                        StringBuilder sb = new();

                        if (serverPacketAttr != null)
                        {
                            sb.AppendLine($"/// {opcodeName}");
                            sb.AppendLine($"WorldPacket const* WorldPackets::Movement::{field.Name}::Write()");
                            sb.Append("{");
                            WriteServerMessageData(ref sb, ref elements);
                            sb.AppendLine();
                            sb.AppendLine("    return &_worldPacket;");
                            sb.AppendLine("}");
                            sb.AppendLine();
                            serverPacketStringBuilder.Append(sb);
                            serverPacketStringBuilder.AppendLine("////////////////////////////////////////////////////////////////////");
                            serverPacketStringBuilder.AppendLine("////////////////////////////////////////////////////////////////////");
                            serverPacketStringBuilder.AppendLine();
                        }
                        else
                        {
                            sb.AppendLine($"/// {opcodeName}");
                            sb.AppendLine($"void WorldPackets::Movement::{field.Name}::Read()");
                            sb.Append("{");
                            WriteClientMessageData(ref sb, ref elements);
                            sb.AppendLine();
                            sb.Append("}");
                            sb.AppendLine();
                            sb.AppendLine();
                            clientPacketStringBuilder.Append(sb);
                            clientPacketStringBuilder.AppendLine("////////////////////////////////////////////////////////////////////");
                            clientPacketStringBuilder.AppendLine("////////////////////////////////////////////////////////////////////");
                            clientPacketStringBuilder.AppendLine();

                            clientPacketHeaderStringBuilder.AppendLine($"class {field.Name} final : public ClientPacket");
                            clientPacketHeaderStringBuilder.AppendLine($"{{");
                            clientPacketHeaderStringBuilder.AppendLine($"public:");
                            clientPacketHeaderStringBuilder.AppendLine($"    {field.Name}(WorldPacket&& packet) : ClientPacket({opcodeName}, std::move(packet)) {{ }}");
                            clientPacketHeaderStringBuilder.AppendLine($"");
                            clientPacketHeaderStringBuilder.AppendLine($"    void Read() override;");
                            clientPacketHeaderStringBuilder.AppendLine($"}};");
                            clientPacketHeaderStringBuilder.AppendLine();

                            forwardDeclarations.AppendLine($"class {field.Name};");

                        }

                    }
                }
            }


            foreach (var field in fields2)
            {
                if (field.FieldType == typeof(MovementStatusElements[]))
                {
                    // Write the array to console
                    var value = field.GetValue(SpellMovementStructures547.Instance);
                    if (value is MovementStatusElements[] elements)
                    {
                        string? opcodeName = null;

                        var serverPacketAttr = field.GetCustomAttribute<ServerPacketAttribute>();
                        var clientPacketAttr = field.GetCustomAttribute<ClientPacketAttribute>();

                        if (serverPacketAttr != null)
                            opcodeName = serverPacketAttr.Opcode;
                        else if (clientPacketAttr != null)
                            opcodeName = clientPacketAttr.Opcode;

                        StringBuilder sb = new();

                        if (serverPacketAttr != null)
                        {
                            sb.AppendLine($"/// {opcodeName}");
                            sb.AppendLine($"WorldPacket const* WorldPackets::Movement::{field.Name}::Write()");
                            sb.Append("{");
                            WriteServerMessageData(ref sb, ref elements);
                            sb.AppendLine();
                            sb.AppendLine("    return &_worldPacket;");
                            sb.AppendLine("}");
                            sb.AppendLine();
                            sb.AppendLine("////////////////////////////////////////////////////////////////////");
                            sb.AppendLine("////////////////////////////////////////////////////////////////////");
                            sb.AppendLine();
                        }
                        else
                        {
                            sb.AppendLine($"/// {opcodeName}");
                            sb.AppendLine($"void WorldPackets::Movement::{field.Name}::Read()");
                            sb.Append("{");
                            WriteClientMessageData(ref sb, ref elements);
                            sb.AppendLine();
                            sb.Append("}");
                            sb.AppendLine();
                            sb.AppendLine();
                            sb.AppendLine("////////////////////////////////////////////////////////////////////");
                            sb.AppendLine("////////////////////////////////////////////////////////////////////");
                            sb.AppendLine();
                        }

                        using (StreamWriter writer = new StreamWriter($"{field.Name}.cpp", false))
                        {
                            writer.WriteLine(sb);
                        }
                    }
                }
            }

            using (StreamWriter writer = new StreamWriter("MovementPacketsServer.cpp", false))
            {
                writer.WriteLine(serverPacketStringBuilder);
            }

            using (StreamWriter writer = new StreamWriter("MovementPacketsClient.cpp", false))
            {
                writer.WriteLine(clientPacketStringBuilder);
            }

            using (StreamWriter writer = new StreamWriter("MovementPackets.h", false))
            {
                writer.WriteLine(clientPacketHeaderStringBuilder);
                writer.WriteLine(serverPacketHeaderStringBuilder);
            }

            using (StreamWriter writer = new StreamWriter("Declarations.h", false))
            {
                writer.WriteLine(forwardDeclarations);
            }

            Console.WriteLine("Dumped");
        }
    }
}
