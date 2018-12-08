pipeline {
  agent any
  stages {

    stage('Build') {
      steps {
        // Compile 
        dir('src') {
          powershell './Build/Build_Win32.ps1'
        }
      }
    }

    stage('Package') {
      steps {
        // Generate the installer.
        dir('src/SovereignClientSetup') {
          powershell './BuildInstaller.ps1'
        }

        // Archive the build artifacts.
        archiveArtifacts 'src/SovereignClientSetup/**/*.msi'
      }
    }

  }
}