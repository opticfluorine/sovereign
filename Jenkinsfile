pipeline {
  agent any
  stages {
    stage('Checkout') {
      steps{
        checkout([$class: 'GitSCM', branches: [[name: '*/master']], doGenerateSubmoduleConfigurations: false, extensions: [[$class: 'SubmoduleOption', disableSubmodules: false, parentCredentials: false, recursiveSubmodules: true, reference: '', trackingSubmodules: false], [$class: 'CleanBeforeCheckout'], [$class: 'GitLFSPull']], submoduleCfg: [], userRemoteConfigs: [[credentialsId: 'fc79acce-aa7d-4b2f-a6ec-a22c1f67ccb7', url: 'ssh://git@gitlab.com/opticfluorine/engine8.git']]])
      }
    }

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