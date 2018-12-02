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
  }
}