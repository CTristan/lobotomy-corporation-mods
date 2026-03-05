// SPDX-License-Identifier: MIT

namespace LobotomyPlaywright.Commands
{
    /// <summary>
    /// Routes command requests to the appropriate handler.
    /// Implemented in Phase 3.
    /// </summary>
    public static class CommandRouter
    {
        public static Protocol.Response HandleCommand(Protocol.Request request)
        {
            return Protocol.Response.CreateError(
                request.Id,
                "Commands are not yet implemented (Phase 3)",
                "NOT_IMPLEMENTED"
            );
        }
    }
}
