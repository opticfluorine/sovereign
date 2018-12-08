pipeline {
  agent any
  stages {

    stage('Checkout') {
      steps {
        checkout([  $class: 'GitSCM', 
          branches: [[name: 'refs/heads/'+env.BRANCH_NAME]],
            doGenerateSubmoduleConfigurations: false,
            extensions: [
                [$class: 'GitLFSPull'],
                [$class: 'CheckoutOption', timeout: 20],
                [$class: 'CloneOption',
                        depth: 0,
                        noTags: false,
                        reference: '/other/option',
                        shallow: false,
                        timeout: 120]
            ],
            submoduleCfg: [],
            userRemoteConfigs: [
                [credentialsId: 'git',
                url: 'ssh://git@gitlab.com/opticfluorine/engine8.git']
            ]
        ])
      }
    }

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