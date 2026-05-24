const { contextBridge, shell } = require('electron');

contextBridge.exposeInMainWorld('desktopInfo', {
  platform: process.platform,
  openPath: async (targetPath) => {
    if (!targetPath) return false;
    const result = await shell.openPath(targetPath);
    return result === '';
  },
  showItemInFolder: (targetPath) => {
    if (!targetPath) return false;
    shell.showItemInFolder(targetPath);
    return true;
  }
});
