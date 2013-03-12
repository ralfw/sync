Task Default -depends Build

Task Build {
   Exec { msbuild "..\source.contracts\sync.contracts\sync.contracts.sln" }

   Exec { msbuild "..\source\sync.localfilesystem\sync.localfilesystem.sln" }
   Exec { msbuild "..\source\sync.remotefilestore.filesystem\sync.remotefilestore.filesystem.sln" }
   Exec { msbuild "..\source\sync.remotesynctable.filesystem\sync.remotesynctable.filesystem.sln" }

   Exec { msbuild "..\source\sync.remotefilestore.parse\sync.remotefilestore.parse.sln" }
      
   Exec { msbuild "..\source\sync.ignore\sync.ignore.sln" }
   Exec { msbuild "..\source\sync.conflicts\sync.conflicts.sln" }
   Exec { msbuild "..\source\sync.localsynctable\sync.localsynctable.sln" }
   Exec { msbuild "..\source\sync.ui\sync.ui.sln" }

   Exec { msbuild "..\source.application\sync.push\sync.push.sln" }
   Exec { msbuild "..\source.application\sync.pull\sync.pull.sln" }
}
