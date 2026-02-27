namespace VisioCall.Shared.Models;

public record IceCandidate(string Candidate, string? SdpMid, int SdpMLineIndex);
