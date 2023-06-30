const path = require('path');

module.exports = {
  mode: 'production',
  entry: '../../../HybridPlaceholder/UserInterface/HybridPlaceholder.jsx',
  output: {
    path: path.resolve('dist'),
    filename: 'index.js',
    libraryTarget: 'commonjs2',
  },
  module: {
    rules: [
      {
        test: /\.jsx?$/,
        exclude: /(node_modules)/,
        use: 'babel-loader',
      },
    ],
  },
  resolve: {
    extensions: ['.js', '.jsx'],
  },
  externals: {
    "@sitecore-jss/sitecore-jss": "@sitecore-jss/sitecore-jss",
    "@sitecore-jss/sitecore-jss/layout": "@sitecore-jss/sitecore-jss/layout",
    "@sitecore-jss/sitecore-jss-react": "@sitecore-jss/sitecore-jss-react",
    "react": "react"
  },
};
