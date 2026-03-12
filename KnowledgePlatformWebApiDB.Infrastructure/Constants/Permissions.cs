using System;
using System.Collections.Generic;
using System.Text;

namespace KnowledgePlatformWebApiDB.Infrastructure.Constants;


public static class Permissions
{
    public const string AddProject = "AddProject";
    public const string RemoveProject = "RemoveProject";

    public const string AddTeam = "AddTeam";
    public const string RemoveTeam = "RemoveTeam";

    public const string AddMember = "AddMember";
    public const string RemoveMember = "RemoveMember";

    public const string ReadNote = "ReadNote";
    public const string WriteNote = "WriteNote";


    public static IReadOnlyCollection<string> All =>
    [
        AddProject,
        RemoveProject,
        AddTeam,
        RemoveTeam,
        AddMember,
        RemoveMember,
        ReadNote,
        WriteNote
    ];
}

//used inside the seeder....