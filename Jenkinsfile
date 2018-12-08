pipeline {
  agent any
  stages {

    stage('Checkout') {
      checkout([  $class: 'GitSCM', 
      branches: [[name: 'refs/heads/'+env.BRANCH_NAME]],
          doGenerateSubmoduleConfigurations: false,
          extensions: [
              [$class: 'GitLFSPull'],
              [$class: 'CheckoutOption', timeout: 20],
              [$class: 'CloneOption',
                      depth: 0,
                      noTags: false,
                      reference: '/other/optional/local/reference/clone',
                      shallow: false,
                      timeout: 120]
          ],
          submoduleCfg: [],
          userRemoteConfigs: [
              [credentialsId: 'foobar',
              url: 'https://github.com/foo/bar.git']
          ]
      ])
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