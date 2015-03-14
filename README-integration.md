----------- Integrating with TortoiseHG ------------


### Step-1
### C:\Program Files\TortoiseHg\hgrc.d\MergeTools.rc

sverge.executable=C:/csharp/Merge/Sverge/bin/Debug/Sverge.exe
sverge.priority=100
sverge.args=/3 /a2 /wait /merge /title1:"Parent 1" /title2:"Merge Result: "$output /title3:"Parent 2" $local $base $other $output
sverge.premerge=False
sverge.checkconflicts=True
sverge.binary=False
sverge.gui=True
sverge.diffargs=/2 /wait /title1:"$plabel1" /title2:"$clabel" $parent $child
sverge.diff3args=/3 /a2 /wait /title1:"$plabel1" /title2:"$clabel" /title3:"$plabel2" $parent1 $child $parent2
sverge.dirdiff=True
sverge.dir3diff=True

### Step-2
Global settings -> Visual Diff Tool
                -> Three-way Merge Tool



----------- Integrating with GIT on Windows ------------

### Step-1
### C:\Program Files (x86)\Git\libexec\git-core\mergetools\sverge

diff_cmd () {
   "$merge_tool_path" "$LOCAL" "$REMOTE" >/dev/null 2>&1
}

merge_cmd () {
   touch "$BACKUP"
   if $base_present
   then
      "$merge_tool_path" \
         "$BASE" "$LOCAL" "$REMOTE" "$MERGED" >/dev/null 2>&1
   else
      "$merge_tool_path" \
         "$LOCAL" "$REMOTE" "$MERGED" >/dev/null 2>&1
   fi
   check_unchanged
}

### Step-2
### C:\Users\svecon\.gitconfig

[diff]
   tool = sverge
[difftool "sverge"]
   path = C:\\csharp\\Merge\\Sverge\\bin\\Debug\\Sverge.exe
[merge]
   tool = sverge
[mergetool "sverge"]
   path = C:\\csharp\\Merge\\Sverge\\bin\\Debug\\Sverge.exe
[difftool]
   prompt = false

### Step-3
git difftool
git difftool -d