import maya.cmds as cmds
folderPath = 'C:/Program Files/Autodesk/Maya2018/bin/plug-ins/InazumaTool/'
dllFileList = cmds.getFileList(folder=folderPath)
if len(dllFileList)>0:
    newestFile = dllFileList[0]
    newestDate = -1
    newestTime = -1
    for dllFile in dllFileList:
        timeParts = dllFile.split('_')
        if not timeParts[0] == 'InazumaTool':
            continue
        if len(timeParts)==1:
            if newestDate < 0:
                newestFile = dllFile
            continue
        #print timeParts
        #date
        dllDate = int(timeParts[1])           
        dllTime = int(''.join(dllFile.split('_')[2:4]))     
        if dllDate>newestDate:
            #yeah 
            newestFile = dllFile
            newestDate = dllDate
            newestTime = dllTime
        elif dllDate == newestDate and dllTime>newestTime:
            newestFile = dllFile
            newestDate = dllDate
            newestTime = dllTime
    resultPath = folderPath+newestFile
    print 'load:',resultPath
    cmds.loadPlugin(resultPath)
    