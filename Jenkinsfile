pipeline {
  agent any
  stages {

    stage('Build') {
      steps {
        dir('src') {
          powershell './Build/Build_Win32.ps1'
        }
      }
    }

    stage('Package') {
      steps {
        dir('src/SovereignClientSetup') {
          
          powershell './BuildInstaller.ps1'

        }
      }
    }

  }
}