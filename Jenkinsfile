#!groovy

node('AspComponents') {
    try {
        deleteDir();
        stage('Checkout') {
              git url: 'http://gitlab.syncfusion.com/essential-studio/ej2-groovy-scripts.git', branch: 'master', credentialsId: env.JENKINS_CREDENTIAL_ID;
              shared = load 'src/shared.groovy';
            checkout scm;
        }

        if(checkCommitMessage()) {
            stage('Install') {
                runShell('git config --global user.email "essentialjs2@syncfusion.com"');
                runShell('git config --global user.name "essentialjs2"');
                runShell('git config --global core.longpaths true');
                runShell('npm -v');
                runShell('npm install');
            }

            stage('Build') {
                  // Compile ASP.Net Core Build
                runShell('dotnet restore ./EJ2ASPCoreFileProvider.csproj');
                //runShell('gulp change-nuspec --nuspec "./EJ2ASPCoreFileProvider.nuspec"');
                runShell('dotnet build ./EJ2ASPCoreFileProvider.csproj /p:Configuration=Debug');
                //deployPackage('EJ2ASPCoreFileProvider.nuspec');
                runShell('gulp clean && git reset --hard');

            }

            stage('Publish') {
                if(shared.isProtectedBranch())  {
                    archiveArtifacts artifacts: 'Nuget/', excludes: null;
                    runShell('gulp publish-nuget');
                }

            }
        }
    }

     catch(Exception e) {
        archiveArtifacts artifacts: '/bin,Nuget/', excludes: null;
        deleteDir();
        error(e);
    }
}

def runShell(String command) {
    if(isUnix()) {
        sh command;
    }
    else {
        bat command;
    }
}

// Check commit message for build setup
def checkCommitMessage() {
    def msg = executeCommand("git log -1 --pretty=%%B");
    println('Commit Message: ' + msg);
    if(msg.indexOf('[ci skip]') != -1) {
        println('CI SKIPPED');
        return false;
    }
    return true;
}

// Execute commands and retrieve the output
def executeCommand(String command) {
    if (isUnix()) {
        sh(script: 'set +x && ' + command + ' > tempFile', returnStdout: true);
    }
    else {
        bat(script: command + ' > tempFile', returnStdout: true);
    }    
    return readFile('tempFile').trim();
}

def deployPackage(String configFile) {
    if(shared.isProtectedBranch()) {
        def packageJson = readFile('package.json').trim();
        def packVersion = new groovy.json.JsonSlurperClassic().parseText(packageJson).version;
        runShell("nuget pack ./$configFile -Properties Configuration=Debug -OutputDirectory ./Nuget -Version $packVersion");
    }
}
